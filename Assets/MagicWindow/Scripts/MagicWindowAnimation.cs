using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicWindow
{
    public class MagicWindowAnimation : MonoBehaviour
    {
        public enum EState
        {
            Unstencilled,
            Preview,
            Volume,
            Window,
            Icon,
        }

        public MagicWindow magicWindow;
        public Transform windowScale;
        public Transform content;
        public Transform iconPivot;
        public Renderer preview;

        public EState nextState = EState.Unstencilled;
        private EState state = EState.Unstencilled;
        public bool enablePreview;
        public bool enablePreviewFill;
        public bool enableVolume;
        public bool enableIcon;
        public bool enableStencil;

        public Vector2 iconRange = new Vector2(0.05f, 1);

        public float volumeLerpTime = 1;
        private float volumeLerp = 1;
        private float volumeValue = 1;

        public float scaleLerpTime = 1;
        private float iconLerp = 1;
        private float iconValue = 1;

        public float previewLerpTime = 0.5f;
        public Vector2 previewFillRange = new Vector2(3, 255);
        private float previewLerp = 0;
        private float previewValue = 0;

        private float previewFillLerp = 0;
        private float previewFillValue = 0;

        private float IconValue(bool on) => (on ? 0 : 1);
        private float VolumeValue(bool on) => (on ? 1 : 0);
        private float PreviewValue(bool on) => (on ? 1 : 0);
        private float PreviewFillValue(bool on) => (on ? 1 : 0);

        private float IconTarget => IconValue(enableIcon);
        private float VolumeTarget => VolumeValue(enableVolume);
        private float PreviewTarget => PreviewValue(enablePreview);
        private float PreviewFillTarget => PreviewFillValue(enablePreviewFill);

        private bool IsIcon => iconLerp == IconValue(true);
        private bool IsWindow => volumeLerp == VolumeValue(false);
        private bool IsVolume => volumeLerp == VolumeValue(true);
        private bool IsPreview => previewLerp == PreviewValue(true);
        private bool IsPreviewFill => previewFillLerp == PreviewFillValue(true);

        [ContextMenu("Icon")] public void GotoIconState() => GotoState(EState.Icon);
        [ContextMenu("Window")] public void GotoWindoState() => GotoState(EState.Window);
        [ContextMenu("Preview")] public void GotoPreviewState() => GotoState(EState.Preview);
        [ContextMenu("Volume")] public void GotoVolumeState() => GotoState(EState.Volume);
        [ContextMenu("Free")] public void GotoFreeState() => GotoState(EState.Unstencilled);

        private IEnumerable GetAnimCoroutine(EState state)
        {
            var co = Enumerable.Empty<object>();

            if (state == EState.Unstencilled)
            {
                SetPreviewFill(PreviewFillValue(false));

                return co
                    .Concat(AnimIcon(false))
                    .Concat(AnimPreview(true, false))
                    .Concat(AnimVolume(true))
                    .Concat(AnimPreview(true, true))
                    .Concat(AnimToStencil(false))
                    .Concat(AnimPreview(false, true));
            }

            // all other states require stencil
            if (!magicWindow.UseStencil)
            {
                SetPreviewFill(PreviewFillValue(true));
                SetVolume(VolumeValue(true));

                co = co
                    .Concat(AnimPreview(true, true))
                    .Concat(AnimToStencil(true))
                    .Concat(AnimPreview(true, false));
            }

            if (state != EState.Icon)
            {
                co = co.Concat(AnimIcon(false));
            }

            switch (state)
            {
                case EState.Icon:
                    co = co
                        .Concat(AnimVolume(false))
                        .Concat(AnimIcon(true));
                    break;

                case EState.Window:
                    if (!IsWindow)
                    {
                        co = co
                            .Concat(AnimPreview(true, false));
                    }

                    co = co
                        .Concat(AnimVolume(false))
                        .Concat(AnimPreview(false, false));
                    break;

                case EState.Volume:
                    if (!IsVolume)
                    {
                        co = co
                            .Concat(AnimPreview(true, false));
                    }

                    co = co
                        .Concat(AnimVolume(true))
                        .Concat(AnimPreview(false, false));
                    break;
            }

            return co;
        }

        private IEnumerable<object> AnimVolume(bool toVolume)
        {
            enableVolume = toVolume;
            while (volumeLerp != VolumeTarget)
            {
                yield return true;
            }
        }

        private IEnumerable<object> AnimIcon(bool toIcon)
        {
            enableIcon = toIcon;
            while (iconLerp != IconTarget)
            {
                yield return true;
            }
        }

        private IEnumerable<object> AnimPreview(bool visible, bool fill)
        {
            enablePreview = visible;
            enablePreviewFill = fill;
            while ((previewLerp != PreviewTarget) || (previewFillLerp != PreviewFillTarget))
            {
                yield return true;
            }
        }

        private IEnumerable<object> AnimToStencil(bool stencil)
        {
            enableStencil = stencil;
            yield return true;
        }

        public void GotoState(EState newState)
        {
            if (state != newState)
            {
                nextState = newState;
                state = newState;
                StopAllCoroutines();
                StartCoroutine(GetAnimCoroutine(newState).GetEnumerator());
            }
        }

        private void Update()
        {
            GotoState(nextState);

            if (Input.GetKeyDown(KeyCode.I))
            {
                GotoState(EState.Icon);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                GotoState(EState.Volume);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                GotoState(EState.Window);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                GotoState(EState.Unstencilled);
            }

            UpdateState();
            UpdateStencil();
            UpdateIcon();
            UpdateVolume();
            UpdatePreview();
        }

        private void UpdateState()
        {
            if (state != nextState)
            {
                GotoState(nextState);
            }
        }

        private void UpdateStencil()
        {
            magicWindow.UseStencil = enableStencil;
            magicWindow.window.gameObject.SetActive(enableStencil);
            magicWindow.hollow.gameObject.SetActive(enableStencil);
        }

        private void UpdateIcon()
        {
            TickScaleTowards(IconTarget);
        }

        private void UpdateVolume()
        {
            TickDepthTowards(VolumeTarget);
        }

        private void UpdatePreview()
        {
            var targetLerp = PreviewTarget;
            var newPreviewLerp = Mathf.MoveTowards(previewLerp, targetLerp, Time.deltaTime / previewLerpTime);
            if (newPreviewLerp != previewLerp)
            {
                previewLerp = newPreviewLerp;
                previewValue = Mathf.SmoothStep(0, 1, previewLerp);
                preview.material.color = new Color(1, 1, 1, previewValue);

                preview.gameObject.SetActive(previewLerp > 0);
            }

            var targetFillLerp = PreviewFillTarget;
            var newPreviewFillLerp = Mathf.MoveTowards(previewFillLerp, targetFillLerp, Time.deltaTime / previewLerpTime);
            if (newPreviewFillLerp != previewFillLerp)
            {
                SetPreviewFill(newPreviewFillLerp);
            }
        }

        private void SetPreviewFill(float newPreviewFillLerp)
        {
            previewFillLerp = newPreviewFillLerp;
            previewFillValue = Mathf.SmoothStep(previewFillRange.x, previewFillRange.y, previewFillLerp);
            preview.material.SetVector("_Range", new Vector4(-255, previewFillRange.x, -255, previewFillValue));
        }

        private void TickScaleTowards(float target)
        {
            SetScale(Mathf.MoveTowards(iconLerp, target, Time.deltaTime / scaleLerpTime));
        }

        private void SetScale(float newScale)
        {
            iconLerp = newScale;
            iconValue = Mathf.SmoothStep(iconRange.x, iconRange.y, iconLerp);

            iconPivot.localScale = Vector3.one * iconValue;
        }
        
        private void TickDepthTowards(float target)
        {
            SetVolume(Mathf.MoveTowards(volumeLerp, target, Time.deltaTime / volumeLerpTime));
        }

        private void SetVolume(float newDepth)
        {
            volumeLerp = newDepth;
            volumeValue = Mathf.SmoothStep(0, 1, volumeLerp);

            var s = windowScale.localScale;
            s.z = volumeValue;
            windowScale.localScale = s;
        }
    }
}