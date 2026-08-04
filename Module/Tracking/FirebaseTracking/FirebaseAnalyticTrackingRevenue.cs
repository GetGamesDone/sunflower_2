using System;
#if VIRTUESKY_FIREBASE_ANALYTIC
using Firebase.Analytics;
#endif

namespace VirtueSky.Tracking
{
    public struct FirebaseAnalyticTrackingRevenue
    {
        public static Action OnTracked;
        public static bool autoTrackAdImpressionAdmob;

        /// <summary>Unused as of the ad_impression tracking consolidation (see VirtueSky.Tracking.Trackings.AdImpression, which now owns Firebase's ad_impression logging at the app level) - kept only because autoTrackAdImpressionAdmob is still written by AdmobClient.cs. Not deleted since other projects sharing this framework module may still call it directly.</summary>
        public static void FirebaseAnalyticTrackRevenue(double value, string network, string unitId,
            string format, string currentAdMediation)
        {
#if VIRTUESKY_FIREBASE_ANALYTIC
            string ad_platform = "";
            switch (currentAdMediation.ToLower())
            {
                case "admob":
                    if (autoTrackAdImpressionAdmob) return;
                    ad_platform = "Admob";
                    break;
                case "applovin":
                    ad_platform = "AppLovin";
                    break;
                case "levelplay":
                    ad_platform = "IronSource";
                    break;
            }

            Parameter[] parameters =
            {
                new("ad_format", format),
                new("ad_platform", ad_platform),
                new("ad_source", network),
                new("ad_unit_name", unitId),
                new("value", value),
                new("currency", "USD"),
            };

            FirebaseAnalytics.LogEvent("ad_impression", parameters);
            OnTracked?.Invoke();
#endif
        }
    }
}