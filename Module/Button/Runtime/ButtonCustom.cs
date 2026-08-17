using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using VirtueSky.Audio;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Misc;
using VirtueSky.Tweening;
using VirtueSky.Utils;
using Button = UnityEngine.UI.Button;


namespace VirtueSky.UIButton
{
    [EditorIcon("icon_button")]
    public abstract class ButtonCustom : Button
    {
        [HeaderLine("Motion", false, CustomColor.Aquamarine, CustomColor.Bright)] [SerializeField]
        private bool invokeClickButton = true;

        [HeaderLine("Motion", false, CustomColor.Aquamarine, CustomColor.Bright)] [SerializeField]
        private bool isMotion = true;

        [SerializeField] private Ease easingTypes = Ease.OutQuint;

        [SerializeField] private float scale = 0.9f;
        [SerializeField] private float timeScale = 0.15f;
        [SerializeField] private bool isShrugOver;
        [SerializeField] private float timeShrug = .2f;
        [SerializeField] private float strength = .2f;

        [HeaderLine("Sound FX", false, CustomColor.Aquamarine, CustomColor.Bright)] [SerializeField]
        private bool useSoundFx;

        [SerializeField] private SoundData soundClickButton;

        Vector3 originScale = Vector3.one;
        private bool canShrug = true;
        private TweenHandle _tweenHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            originScale = transform.localScale;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SnapScale();
        }


        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            // Not clickable - skip the punch-scale motion and sfx too, instead of playing them for a
            // press that can't actually trigger onClick. IsInteractable() (not just the interactable
            // field) also accounts for a disabled parent CanvasGroup.
            if (!IsInteractable()) return;

            DoScale();
            if (invokeClickButton)
            {
                ButtonStatic.OnClickButtonEvent?.Invoke();
            }

            if (useSoundFx)
            {
                soundClickButton.PlaySfx();
            }
        }


        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            ResetScale();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (!IsInteractable()) return;
            Shrug();
        }

        void DoScale()
        {
            if (!isMotion) return;

            // Cancel (not Complete) any tween still running from a previous fast tap - Cancel just
            // stops it in place instead of snapping to its end value first, and starting the new
            // tween from the current localScale (not always originScale) keeps the motion
            // continuous instead of jumping, which is what was causing the jitter.
            if (_tweenHandle.IsActive) _tweenHandle.Cancel();

            _tweenHandle = Tween.Create(transform.localScale, originScale * scale, timeScale).WithEase(easingTypes).BindToLocalScale(transform);
        }

        void Shrug()
        {
            if (isMotion && isShrugOver && canShrug)
            {
                canShrug = false;
                if (isMotion && isShrugOver)
                {
                    transform.Shrug(timeShrug, strength, Ease.OutQuad, () => { canShrug = true; });
                }
            }
        }

        // Animates back to originScale from wherever the press tween currently is - Cancel (not
        // Complete) so a fast tap/release doesn't force a snap to the pressed scale first, which
        // read as a jerky double-motion. Tweening from the live transform.localScale keeps this
        // continuous with whatever the press animation was doing when it got interrupted.
        void ResetScale()
        {
            if (!isMotion) return;

            if (_tweenHandle.IsActive) _tweenHandle.Cancel();

            _tweenHandle = Tween.Create(transform.localScale, originScale, timeScale).WithEase(easingTypes).BindToLocalScale(transform);
        }

        // Instant, non-animated reset used when the button is being disabled - the object may be
        // hidden/destroyed right after, so it must not be left mid-tween (see OnDisable).
        void SnapScale()
        {
            if (!isMotion) return;

            if (_tweenHandle.IsActive) _tweenHandle.Cancel();

            transform.localScale = originScale;
        }
    }
}