using System;
using System.Collections;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;

#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID && VIRTUESKY_RATING
using Google.Play.Review;

#endif

namespace VirtueSky.Rating
{
    [EditorIcon("icon_manager"), HideMonoScript]
    public class InAppReviewManager : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad;
        [Space, SerializeField] private bool autoInit;
        private bool isInternalRatingInitialize { get; set; }

#if UNITY_ANDROID && VIRTUESKY_RATING
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        private Coroutine _coroutine;
#endif

        private static event Func<bool> OnGetIsInitializeEvent;
        private static event Action OnInitRateAndReviewEvent;
        private static event Action OnRateAndReviewEvent;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            OnGetIsInitializeEvent += InternalGetRatingInitialize;
            OnInitRateAndReviewEvent += InternalInitRateAndReview;
            OnRateAndReviewEvent += InternalRateAndReview;
        }

        private void OnDisable()
        {
            OnGetIsInitializeEvent -= InternalGetRatingInitialize;
            OnInitRateAndReviewEvent -= InternalInitRateAndReview;
            OnRateAndReviewEvent -= InternalRateAndReview;
        }

        private void Start()
        {
            isInternalRatingInitialize = false;
            if (autoInit)
            {
                InternalInitRateAndReview();
            }
        }

        private bool InternalGetRatingInitialize() => isInternalRatingInitialize;

        private void InternalInitRateAndReview()
        {
            if (isInternalRatingInitialize) return;
            if (!Application.isMobilePlatform) return;
#if UNITY_ANDROID && VIRTUESKY_RATING
            _coroutine = App.StartCoroutine(InitReview());
#endif
            isInternalRatingInitialize = true;
        }

        private void InternalRateAndReview()
        {
            if (!Application.isMobilePlatform) return;

#if UNITY_ANDROID && VIRTUESKY_RATING
            App.StartCoroutine(LaunchReview());
#elif UNITY_IOS
            Device.RequestStoreReview();
#endif
        }

#if UNITY_ANDROID && VIRTUESKY_RATING
        // Play Core's In-App Review API only ever works for apps installed via the Play Store
        // (internal/production track) - on a sideloaded/dev build (installed via adb/USB, which is how
        // this project is normally tested) RequestReviewFlow silently no-ops instead of erroring, so the
        // error-based fallback below never fires. Skip the API entirely for non-Play-Store installs and
        // go straight to the store page instead of chasing that undetectable no-show.
        private static bool IsInstalledFromPlayStore => Application.installerName == "com.android.vending";

        private IEnumerator InitReview(bool force = false)
        {
            if (!IsInstalledFromPlayStore)
            {
                if (force) DirectlyOpen();
                yield break;
            }

            if (_reviewManager == null) _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (force) DirectlyOpen();
                yield break;
            }

            _playReviewInfo = requestFlowOperation.GetResult();
        }

        public IEnumerator LaunchReview()
        {
            if (_playReviewInfo == null)
            {
                if (_coroutine != null) App.StopCoroutine(_coroutine);
                yield return App.StartCoroutine(InitReview(true));
            }

            // InitReview(true) already opened the store link on failure/non-Play-Store install above -
            // nothing left to launch.
            if (_playReviewInfo == null) yield break;

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null;
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                DirectlyOpen();
                yield break;
            }
        }
#endif
        private void DirectlyOpen()
        {
            Application.OpenURL($"https://play.google.com/store/apps/details?id={Application.identifier}");
        }


        public static bool IsInitialize => (bool)OnGetIsInitializeEvent?.Invoke();
        public static void InitRateAndReview() => OnInitRateAndReviewEvent?.Invoke();
        public static void RateAndReview() => OnRateAndReviewEvent?.Invoke();
    }
}