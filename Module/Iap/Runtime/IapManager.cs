#if VIRTUESKY_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using VirtueSky.Ads;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Misc;

namespace VirtueSky.Iap
{
    [EditorIcon("icon_manager"), HideMonoScript]
    public class IapManager : MonoBehaviour
    {
        [SerializeField] private bool isPersistent;

        [Space] [ReadOnly, SerializeField] private List<IapDataProduct> products = new List<IapDataProduct>();
        public static event Action<string> OnPurchaseSucceedEvent;
        public static event Action<string, string> OnPurchaseFailedEvent;

        private StoreController _storeController;
        private bool isRequestBuilder = false;
        private bool flag = false;

        // Product v5 no longer exposes hasReceipt/receipt, so ownership and subscription state
        // are tracked locally from the orders granted via OnPurchasePending/OnPurchaseConfirmed/OnPurchasesFetched.
        private readonly HashSet<string> ownedProductIds = new HashSet<string>();
        private readonly Dictionary<string, Order> ownedOrders = new Dictionary<string, Order>();

        public static bool IsExist => instance != null;
        public static bool IsInitialized { get; private set; }

        private static IapManager instance;

        private void Awake()
        {
            if (isPersistent)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CreateIapProducts();
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Awake ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake)
            {
                InternalInitialization();
            }
        }

        private void OnEnable()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable)
            {
                InternalInitialization();
            }
        }

        private void Start()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Start ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Start)
            {
                InternalInitialization();
            }
        }

        private async void InternalInitialization()
        {
            if (IsInitialized || flag) return;
            flag = true;
            while (!UnityServiceInitialization.IsUnityServiceReady)
            {
                await Task.Delay(50);
            }

            _storeController = UnityIAPServices.StoreController();

            // Subscribe to every event before Connect() - pending purchases from a previous
            // session can fire immediately on reconnect.
            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
            _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            
            await _storeController.Connect();

            RequestProductData();

            Debug.Log("IapManager initialized!".SetColor(Color.cyan));
        }

        private void CreateIapProducts()
        {
            foreach (var iapData in IapSettings.SkusData)
            {
                if (IsDuplicateProduct(iapData.Id))
                {
                    Debug.LogWarning($"[IapManager] Duplicate product id '{iapData.Id}' in skusData. Skipping.");
                    continue;
                }

                var product = new IapDataProduct(iapData.androidId, iapData.iosId, iapData.productType, iapData.priceConfig);
                products.Add(product);
            }
        }

        private bool IsDuplicateProduct(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var p in products)
            {
                if (p.Id == id) return true;
            }

            return false;
        }

        #region Store Callbacks

        private void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
        {
            Debug.LogWarning($"[IapManager] Store disconnected: {failureDescription.Message}");
        }

        private void OnProductsFetched(List<Product> fetchedProducts)
        {
            IsInitialized = true;
            _storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"[IapManager] Product fetch failed: {failure.FailureReason}");
        }

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var confirmedOrder in orders.ConfirmedOrders)
            {
                GrantOwnership(confirmedOrder);
            }
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"[IapManager] Purchases fetch failed: {failure.Message}");
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            Debug.Log("[IapManager] Purchase deferred, awaiting approval (e.g. Ask-to-Buy).");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            if (IsPurchaseValid(order))
            {
                _storeController.ConfirmPurchase(order);
            }
            else
            {
                InternalPurchaseFailed(GetOrderProductId(order), PurchaseFailureReason.Unknown);
            }
        }

        private void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case ConfirmedOrder confirmedOrder:
                    GrantOwnership(confirmedOrder);
                    PurchaseVerified(confirmedOrder);
                    break;
                case FailedOrder failedOrder:
                    InternalPurchaseFailed(GetOrderProductId(failedOrder), failedOrder.FailureReason);
                    break;
            }
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            InternalPurchaseFailed(GetOrderProductId(order), order.FailureReason);
        }

        #endregion

        private static string GetOrderProductId(Order order) => order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;

        private bool IsPurchaseValid(PendingOrder order)
        {
            if (!IapSettings.IsValidatePurchase) return true;

            if (IapSettings.IsCustomValidatePurchase && IapSettings.ValidatePurchase != null)
            {
                return IapSettings.ValidatePurchase.IsValidate();
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Apple local receipt validation is a no-op under StoreKit 2, so CrossPlatformValidator
            // is only meaningful for Google Play here.
            var validator = new UnityEngine.Purchasing.Security.CrossPlatformValidator(
                UnityEngine.Purchasing.Security.GooglePlayTangle.Data(), Application.identifier);
            try
            {
                validator.Validate(order.Info.Receipt);
                return true;
            }
            catch (UnityEngine.Purchasing.Security.IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                return false;
            }
#else
            return true;
#endif
        }

        private void GrantOwnership(Order order)
        {
            var id = GetOrderProductId(order);
            if (string.IsNullOrEmpty(id)) return;
            ownedProductIds.Add(id);
            ownedOrders[id] = order;
        }

        private void PurchaseProductInternal(IapDataProduct product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _storeController?.PurchaseProduct(product.Id);
#elif UNITY_EDITOR
            InternalPurchaseSuccess(product.Id);
#endif
        }

        private void RequestProductData()
        {
            if (isRequestBuilder) return;
            isRequestBuilder = true;
            var productDefinitions = new List<ProductDefinition>(products.Count);
            foreach (var p in products)
            {
                productDefinitions.Add(new ProductDefinition(p.Id, ConvertProductType(p.iapProductType)));
            }

            _storeController.FetchProducts(productDefinitions);
        }

        private void InternalPurchaseFailed(string id, PurchaseFailureReason failureReason)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            foreach (var product in products)
            {
                if (product.Id != id) continue;
                OnPurchaseFailedEvent?.Invoke(product.Id, failureReason.ToString());
                Common.CallActionAndClean<string>(ref product.purchaseFailedCallback, failureReason.ToString());
            }
        }

        void PurchaseVerified(Order order)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            InternalPurchaseSuccess(GetOrderProductId(order));
        }

        void InternalPurchaseSuccess(string id)
        {
            foreach (var product in products)
            {
                if (product.Id != id) continue;
                OnPurchaseSucceedEvent?.Invoke(product.Id);
                Common.CallActionAndClean(ref product.purchaseSuccessCallback);
            }
        }


#if UNITY_IOS
        private void InternalRestorePurchase()
        {
            if (!IsInitialized)
            {
                Debug.Log("Restore purchases fail. not initialized!");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("Restore purchase started ...");

                _storeController.RestoreTransactions((success, error) =>
                {
                    Debug.Log(success
                        ? "Restore purchase continuing. If no further messages, no purchase available to restore."
                        : $"Restore purchase failed: {error}");
                });
            }
            else
            {
                Debug.Log("Restore purchase fail. not supported on this platform. current = " + Application.platform);
            }
        }
#endif

        ProductType ConvertProductType(IapProductType iapProductType)
        {
            switch (iapProductType)
            {
                case IapProductType.Consumable:
                    return ProductType.Consumable;
                case IapProductType.NonConsumable:
                    return ProductType.NonConsumable;
                case IapProductType.Subscription:
                    return ProductType.Subscription;
            }

            return ProductType.Consumable;
        }


        #region Internal API

        private IapDataProduct InternalPurchaseProductById(string id)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(true);
            var product = GetIapProduct(id);
            PurchaseProductInternal(product);
            return product;
        }

        private IapDataProduct InternalPurchaseProductByIapData(IapDataProduct product)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(true);
            PurchaseProductInternal(product);
            return product;
        }

        private bool InternalIsPurchasedProductByIapData(IapDataProduct product)
        {
            return ConvertProductType(product.iapProductType) is ProductType.NonConsumable or ProductType.Subscription &&
                   ownedProductIds.Contains(product.Id);
        }

        private bool InternalIsPurchasedProductById(string id)
        {
            var product = GetIapProduct(id);
            return product != null && InternalIsPurchasedProductByIapData(product);
        }

        private Product InternalGetProductByIapData(IapDataProduct product)
        {
            return _storeController?.GetProductById(product.Id);
        }

        private Product InternalGetProductById(string id)
        {
            return _storeController?.GetProductById(id);
        }

        private SubscriptionInfo InternalGetSubscriptionInfo(IapDataProduct product)
        {
            if (ConvertProductType(product.iapProductType) != ProductType.Subscription) return null;
            if (!ownedOrders.TryGetValue(product.Id, out var order)) return null;
            return order.Info.PurchasedProductInfo?.FirstOrDefault(p => p.productId == product.Id)?.subscriptionInfo;
        }

        #endregion

        #region Public API

        public static List<IapDataProduct> Products() =>
            instance != null ? new List<IapDataProduct>().Adds(instance.products) : new List<IapDataProduct>();

        public static IapDataProduct GetIapProduct(string id)
        {
            foreach (var product in instance.products)
            {
                if (product.Id == id) return product;
            }

            return null;
        }

        /// <summary>
        /// Add product to IapManager. You can use this API to add product at runtime, but you need to make sure called before IapManager intialization.
        /// </summary>
        /// <param name="product"></param>
        public static void AddProduct(IapDataProduct product)
        {
            if (instance == null)
            {
                Debug.LogWarning("[IapManager] AddProduct called before IapManager instance is ready.");
                return;
            }

            if (instance.IsDuplicateProduct(product.Id))
            {
                Debug.LogWarning($"[IapManager] Duplicate product id '{product.Id}'. Skipping.");
                return;
            }

            if (instance.isRequestBuilder)
            {
                Debug.LogWarning(
                    "[IapManager] AddProduct called after IapManager has requested product data. The new product will not be included in the initialization.");
                return;
            }

            instance.products.Add(product);
        }

        public static IapDataProduct PurchaseProduct(string id) => instance != null ? instance.InternalPurchaseProductById(id) : null;

        public static IapDataProduct PurchaseProduct(IapDataProduct product) =>
            instance != null ? instance.InternalPurchaseProductByIapData(product) : null;

        public static bool IsPurchasedProduct(IapDataProduct product) => instance != null && instance.InternalIsPurchasedProductByIapData(product);

        public static bool IsPurchasedProduct(string id) => instance != null && instance.InternalIsPurchasedProductById(id);

        public static Product GetProduct(IapDataProduct product) => instance != null ? instance.InternalGetProductByIapData(product) : null;
        public static Product GetProduct(string id) => instance != null ? instance.InternalGetProductById(id) : null;

        public static SubscriptionInfo GetSubscriptionInfo(IapDataProduct product) =>
            instance != null ? instance.InternalGetSubscriptionInfo(product) : null;

#if UNITY_IOS
        public static void RestorePurchase()
        {
            if (instance != null)
            {
                instance.InternalRestorePurchase();
            }
        }
#endif

        public static void Initialization()
        {
            if (instance != null)
            {
                instance.InternalInitialization();
            }
        }

        #endregion
    }
}

#endif
