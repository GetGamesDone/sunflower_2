# Changelog

All notable changes to the **Sunflower2** package (`com.wolf-org.sunflower2`) are documented here.

This file was reconstructed from the package's git history (no changelog previously existed). Entries are grouped by the version recorded in `package.json` at each commit; pre-release `-preview` builds are folded into the release they led up to. Dates are the commit date.

## [1.8.8] - 2026-07-23
- Refactor `GameData` to use generic data storage, introducing `IDataStorage`/`IJsonService` abstractions with `PlayerPrefStorage` and `NewtonsoftJsonService` implementations
- Update new API by Unity 6000.3.20f1

## [1.8.7-preview.1] - 2026-06-29
- Log ads
- Fix ads callback event

## [1.8.7-preview] - 2026-06-24
- Add option for ads to execute callback on the main thread

## [1.8.6] - 2026-06-15
_Includes 1.8.6-preview, 1.8.6-preview.1_
- Backoff/cap for reloading AdMob banners
- Update to new Max/AdMob banner API

## [1.8.5] - 2026-06-09
_Includes 1.8.5-preview, 1.8.5-preview.1_
- Refactor ads revenue tracking
- Don't load banner right after SDK init
- Add `StartSDK` method for `AppsFlyerGameObject`

## [1.8.4] - 2026-06-02
- Fix `IapManager`
- Update package dependencies

## [1.8.3] - 2026-06-02
_Includes 1.8.3-preview through 1.8.3-preview.5_
- Update play SFX/music with clip index
- Update registered package versions
- Update Tween
- Fix saved audio volume
- Add `IsExist` method for Advertising
- Add `IsExist` property for `IapManager`
- Null-check ad client before returning a value

## [1.8.2] - 2026-05-22
- Version bump (no functional changes)

## [1.8.1] - 2026-05-17
_Includes 1.8.1-preview, 1.8.1-preview.1_
- Refactor IAP: centralize IAP initialization and SKU settings
- Add VirtueSky Tweening
- Remove dependency on UniTask and PrimeTween
- Fix Shrug extension
- Rename assembly
- Add more constructor overloads for `AdsInfo` and `AdsError`
- Update IAP

## [1.8.0] - 2026-05-16
- Fix ads
- Add placement `AdsInfo` for `AdmobAdUnit`
- Add fail-reason parameter to IAP purchase failure callback

## [1.7.9] - 2026-05-15
- Fix IAP
- Add abstract `ValidatePurchase` scriptable object for custom purchase validation

## [1.7.8] - 2026-05-14
_Includes 1.7.8-preview_
- Fix `RuntimeInitType` enum
- Fix condition for auto-showing App Open Ad
- Update AdMob ads info
- Call ad unit events on the Unity main thread

## [1.7.7] - 2026-05-09
_Includes 1.7.6.1 (preview test), 1.7.7-preview.0/.1/.2/.3_
- Fix reset of LevelPlay interstitial callback on iOS
- Fix ads error and synchronization issues
- Add `ShowAdMediationDebugger` API for `AdClient`
- Refactor advertising
- Add test suite run before LevelPlay SDK init
- Add Mediation Load Mode option
- Prevent loading a LevelPlay ad while one is already showing
- Fix Advertising magic-panel tab for single Mediation Load Mode
- Call `admob.SetRequestConfiguration` before SDK init

## [1.7.6] - 2026-05-05
- Update `AdStatic`
- Add preview-sound option for sound data
- Fix null LevelPlay reward ad error on load
- Fix Asset Finder cache path
- Update Discord release-notify workflow

## [1.7.5] - 2026-04-24
- Default placement param to `""` and set placement for banner
- Fix add-impression for LevelPlay
- LevelPlay: pause game if SDK initialization completed
- Add `receivedRewardCallback`
- Add Finalize/Close handling for reward ad
- Don't load reward ads if one is already showing

## [1.7.4] - 2026-04-22
- Update `ResizeMatchCanvasScalerComponent`
- Update Discord release-notify workflow

## [1.7.3] - 2026-04-22
- Add `VLog`
- Add `OnVolumeSfxChangedEvent` / `OnVolumeMusicChangedEvent` events

## [1.7.2] - 2026-04-21
- Update `TableColumnWidth` attribute

## [1.7.1] - 2026-04-21
- Add `startDelay` for `EffectAppearComponent`
- Add `isShowOnLoad` toggle for `LevelPlayBannerAd`
- Fix app-tracking `ad_impression` param name
- Add Discord release-notify CI workflow
- Fix and update ads
- Add custom `AdsInfo`/`AdsError` for callbacks
- Update Notification `ScheduleMode`
- Update registered package versions

## [1.7.0] - 2026-03-31
- Minor cleanup / removals

## [1.6.9] - 2026-03-15
- Synchronized with Sunflower framework
- Update registered package versions
- Update Asset Finder

## [1.6.6] - 2026-03-31
- Remove PrimeTween

## [1.6.8] - 2026-01-08
- Fix path for Asset Finder "show window" menu item
- Update PrimeTween to 1.3.7, Tri-Inspector to 1.15.1
- Update registered package versions

## [1.6.7] - 2025-12-04
- Update registered package versions
- Update Asset Finder

## [1.6.6] - 2025-11-13
- Update `IapDataProduct`, update registered package versions
- Update audio control-panel editor
- Update level

## [1.6.5] - 2025-09-29
- Version bump (no functional changes)

## [1.6.4] - 2025-09-25
- Update registered package versions
- Add `timeScale` param for skeleton extension
- Update PrimeTween to 1.3.3, Tri-Inspector to 1.15.0
- Update sound-data method and editor preview
- Update to new LevelPlay API (v9.0.0)

## [1.6.3] - 2025-08-18
- Update dependencies in README
- Update registered package versions
- Add `VIRTUESKY_UNITY_SERVICES` define symbol

## [1.6.2] - 2025-07-27
- Update registered package versions
- Remove ATT iOS module
- Fix ads

## [1.6.1] - 2025-07-20
- Update level editor

## [1.6.0] - 2025-07-15
- Version bump (no functional changes)

## [1.5.9] - 2025-07-11
- Add `useOffsetTrans` option for `FollowTargetComponent`
- Update registered package versions
- Fix authentication

## [1.5.8] - 2025-06-13
- Fix `PostBuildStep`

## [1.5.7] - 2025-06-13
- Fix `PostBuildStep`

## [1.5.6] - 2025-06-12
- Update PrimeTween to 1.3.2
- Update registered package versions

## [1.5.5] - 2025-05-27
- Update registered package versions

## [1.5.4] - 2025-04-22
- Add `Common.Collection`; remove `AddRange` helper
- Fix Spine extension
- Refactor ads
- Remove AppsFlyer revenue generic connector
- Fix lib path
- Update registered package versions
- Fix Common skeleton
- Change button label
- Update AppsFlyer to 6.16.21
- Add `enableTrackAdRevenue` option

## [1.5.3] - 2025-03-09
- Add auto-tracking option for AdMob `ad_impression`
- Update registered package versions
- Add `OnTracked` event for tracking

## [1.5.2] - 2025-02-22
- Remove Max reward-interstitial ads
- Use `UNITY_ANDROID` define symbol for `AndroidExternalToolsSettings`
- Update registered package versions

## [1.5.1] - 2025-01-21
- Fix define-symbol error in `NativeOverlayAdUnit`
- Update registered package versions

## [1.5.0] - 2025-01-14
- Update registered package versions

## [1.4.9] - 2024-12-24
- Update registered package versions
- Fix ads

## [1.4.8] - 2024-12-16
- Update Native Overlay Ads
- Update Adjust SDK to 5.0.6
- Update PrimeTween to 1.2.2
- Update registered package versions
- Add `GetSubscriptionInfo` method for IAP
- Add `IsAutoSave` property for `GameData`
- Update package.json dependencies

## [1.4.7] - 2024-12-04
- Update PrimeTween to 1.2.1
- Add Native Overlay Ad support

## [1.4.6] - 2024-11-22
- Update registered package versions
- Fix registered package / component issues

## [1.4.5] - 2024-11-15
- Fix namespace error
- Update AdMob SDK to 9.4.0
- Fix `AudioManager`

## [1.4.4] - 2024-11-12
- Change list to array
- Update registered package versions
- Add In-App Review tab in magic panel
- Add sound-on-click for custom button

## [1.4.3] - 2024-11-04
- Fix `Texture2D` creation for Unity 6
- Add icon
- Update Firebase to 12.4.0, IronSource SDK to 8.4.0

## [1.4.2] - 2024-10-24
- Version bump (no functional changes)

## [1.4.1] - 2024-10-23
- Fix debug logging
- Refactor ads and IAP
- Refactor `AudioManager`, `InAppReviewManager`, Firebase manager, `TouchInputManager`

## [1.4.0] - 2024-10-22
- Add Linq usage and refactor
- Add Localization module
- Add ping button for Adjust/AppsFlyer config
- Update Max SDK to 8.0.0

## [1.3.9] - 2024-10-14
- Fix `HeaderLine`
- Update UniTask to 2.5.10, UI Particle
- Fix `AdUnit`
- Hide `IronSourceAdUnit` in AdSettings
- Remove `ConfirmPendingPurchase` in `IapManager`
- Add price field to `IapDataProduct` and `PriceConfig` method to `IapProduct`

## [1.3.8] - 2024-10-09
- Update Adjust to v5.0.3, PrimeTween to 1.2.0
- Update Google Mobile Ads to 9.2.1

## [1.3.7] - 2024-10-02
- Fix log error on null audio clip
- Update PrimeTween to 1.2.22
- Fix iOS privacy flow (show GDPR after tracking authorization)

## [1.3.6] - 2024-09-16
- Remove COPPA handling for Max SDK 7.0.0; update Max SDK to 7.0.0

## [1.3.5] - 2024-09-05
- Remove Firebase tracking static
- Fix missing AdRevenue data sent to AppsFlyer
- Update Firebase SDK to 12.2.1
- Add header hierarchy

## [1.3.4] - 2024-08-28
- Refactor Notification module
- Remove tracking static in favor of scriptable tracking
- Add button to create scriptable tracking in magic panel

## [1.3.3] - 2024-08-27
- Version bump (no functional changes)

## [1.3.2] - 2024-08-27
- Refactor Game Service
- Fix repo URL
- Rename Unity-Common package to Sunflower2

## [1.3.1] - 2024-08-23
- Fix missing Firebase define symbols
- Update Max SDK to 6.6.3
- Fix Control Panel
- Remove UniTask module; add button to install UniTask instead

## [1.3.0] - 2024-08-19
- Update PrimeTween to 1.1.20, UI Particle to 4.9.1
- Fix Control Panel drawing
- Add `Common.Math`
- Add `NameEvent` for GPGS and Apple

## [1.2.9] - 2024-08-13
- Add `AdjustSetting` and `AppsFlyerSetting`

## [1.2.8] - 2024-08-13
- Update IAP SDK to 4.12.2, Firebase to 12.2.0
- Update Max SDK to 6.6.2, External Dependency Manager to 1.2.182
- Add `FirebaseDependencyAvailable` property

## [1.2.7] - 2024-08-04
- Add `ResizeCameraOrthographicComponent`

## [1.2.6] - 2024-08-03
- Version bump (no functional changes)

## [1.2.5] - 2024-07-31
- Fix null `FolderIconSettings`

## [1.2.4] - 2024-07-31
- Fix `FolderIcons`

## [1.2.3] - 2024-07-30
- Fix `FolderIcon`

## [1.2.2] - 2024-07-30
- Add Game Service Authentication
- Add `FolderIcon` module

## [1.2.1] - 2024-07-25
- Update Max SDK to 6.6.1, Google AdMob SDK to 9.2.0
- Add `IsCollapsible` API for `AdmobBannerAdUnit`
- Add `PreventTouch` property to `TouchInputManager`
- Stop using singleton for `AudioManager`, `RatingManager`, `NotificationManager`, `FirebaseRemoteConfigManager`

## [1.2.0] - 2024-07-22
- Add `DictionaryCustom`
- Update Max SDK to 6.6.0
- Fix runtime auto-init

## [1.1.9] - 2024-07-21
- Fix `AdSetting` layout
- Add Firebase control panel
- Fix auto-init for ads and IAP
- Update Tri-Inspector to 1.14.1

## [1.1.8] - 2024-07-18
- Add `FollowTargetComponent` + `SetPosition`/`SetRelativePosition` extensions
- Add `ad_impression` tracking for IronSource
- Fix IAP editor product-getter generation

## [1.1.7] - 2024-07-15
- Destroy banner when load fails
- Change define-symbol name
- Add init type for remote config

## [1.1.6] - 2024-07-09
- Fix IronSource

## [1.1.5] - 2024-07-05
- Update AdMob SDK to 9.1.1
- Update touch handling
- Fix Advertising

## [1.1.4] - 2024-07-03
- Update UniTask to 2.5.5, External Dependency Manager to 1.2.181
- Fix Advertising/`IapManager`, `AudioManager`/`AudioHelper`, `RatingManager`/`FirebaseRemoteConfigManager` APIs
- Fix Firebase tracking ATT-result event

## [1.1.3] - 2024-07-02
- Add `isMobilePlatform` condition for Firebase tracking
- Update iOS ads support to 1.2.0
- Add `AudioHelper`
- Fix App Open Ad show condition
- Add Firebase Auth; update Firebase to 12.1.0
- Add IronSource support
- Fix `AdUnit` init callback

## [1.1.2] - 2024-06-24
- Add `LocalizedPriceProduct` for IAP

## [1.1.1] - 2024-06-18
- Update pooling and advertising
- Fix `IapManager` event
- Refactor `AdClient`

## [1.1.0] - 2024-06-13
- Add remote-config timing for notifications
- Refactor pooling; fix pool API

## [1.0.9] - 2024-06-07
- Add AppOpen test `AdUnit`
- Add Firebase iOS support
- Update Adjust SDK to 4.38.0, AppsFlyer SDK to 6.14.3
- Add `ResizeMatchCanvasScalerComponent`
- Add Tracking module
- Add `POST_NOTIFICATIONS` permission for Android

## [1.0.8] - 2024-05-29
- Custom priority for menu items
- Show/hide banner on App Open close/display
- Update PrimeTween to 1.1.19

## [1.0.7] - 2024-05-29
- Fix Advertising, Component
- Rename `AdUnit` event

## [1.0.6] - 2024-05-21
- Add LICENSE
- Fix Max App Open Ad show/auto-show

## [1.0.5] - 2024-05-17
- Update Firebase to 12.0.0, PrimeTween to 1.1.18
- Update In-App Purchasing to 4.11.0, Max SDK to 6.5.1

## [1.0.4] - 2024-05-09
- Add static pool
- Fix audio

## [1.0.3] - 2024-05-08
- Update Max SDK to 6.4.4
- Fix `ButtonCustom` (added `isHandleClickButtonEvent`)
- Add Remote Config module
- Remove Firebase remote editor

## [1.0.2] - 2024-05-03
- Add Unity Package support
- Add `IsReady` check method for `AdUnit`
- Add native Vibration

## [1.0.1] - 2024-05-03
- Add Tri-Inspector, Asset Finder, PrimeTween
- Add custom Button, UniTask, Object Pooling, Push Notification
- Add Audio module, sound data
- Fix tracking
- Add Component/CacheComponent module
- Add Level Editor and Hierarchy tools
- Add Control Panel

## [1.0.0] - 2024-05-02
Initial public release.
- Core singleton framework
- Ads module (AdMob, Max), IAP module (`IapManager`, `PurchaseProduct`)
- Data storage module
