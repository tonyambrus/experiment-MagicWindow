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

        public EState newState = EState.Icon;
        private EState state = EState.Volume;
        public bool enablePreview;
        public bool enableVolume;
        public bool enableIcon;
        public bool enableStencil;

        public float iconScale = 0.05f;

        public float windowLerpTime = 1;
        private float windowDepthLerp = 1;
        private float windowDepth = 1;

        public float scaleLerpTime = 1;
        private float scaleLerp = 1;
        private float scale = 1;

        public float previewLerpTime = 0.5f;
        private float previewLerp = 0;
        private float previewValue = 0;

        [ContextMenu("Icon")] public void GotoIconState() => GotoState(EState.Icon);
        [ContextMenu("Window")] public void GotoWindoState() => GotoState(EState.Window);
        [ContextMenu("Preview")] public void GotoPreviewState() => GotoState(EState.Preview);
        [ContextMenu("Volume")] public void GotoVolumeState() => GotoState(EState.Volume);
        [ContextMenu("Free")] public void GotoFreeState() => GotoState(EState.Unstencilled);

        public void GotoState(EState newState)
        {
            state = newState;
            switch (newState)
            {
                case EState.Unstencilled:
                    enablePreview = false;
                    enableVolume = true;
                    enableStencil = false;
                    enableIcon = false;
                    break;

                case EState.Preview:
                    enableIcon = false;
                    enablePreview = true;
                    break;

                case EState.Volume:
                    enableStencil = true;
                    enableIcon = false;
                    enableVolume = true;
                    break;

                case EState.Window:
                    enablePreview = false;
                    enableStencil = true;
                    enableIcon = false;
                    enableVolume = false;
                    break;

                case EState.Icon:
                    enablePreview = false;
                    enableStencil = true;
                    enableIcon = true;
                    enableVolume = false;
                    break;
            }
        }

        private void Update()
        {
            UpdateState();
            UpdateStencil();
            UpdateIcon();
            UpdateVolume();
            UpdatePreview();
        }

        private void UpdateState()
        {
            if (state != newState)
            {
                GotoState(newState);
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
            TickScaleTowards(enableIcon ? 0 : 1);
        }

        private void UpdateVolume()
        {
            TickDepthTowards(enableVolume ? 1 : 0);
        }

        private void UpdatePreview()
        {
            var targetLerp = enablePreview ? 1 : 0;
            var newPreviewLerp = Mathf.MoveTowards(previewLerp, targetLerp, Time.deltaTime / previewLerpTime);
            if (newPreviewLerp != previewLerp)
            {
                previewLerp = newPreviewLerp;
                previewValue = Mathf.SmoothStep(0, 1, previewLerp);
                preview.material.color = new Color(1, 1, 1, previewValue);

                preview.gameObject.SetActive(previewLerp > 0);
            }
        }

        private void TickScaleTowards(float target)
        {
            SetScale(Mathf.MoveTowards(scaleLerp, target, Time.deltaTime / scaleLerpTime));
        }

        private void SetScale(float newScale)
        {
            scaleLerp = newScale;
            scale = Mathf.SmoothStep(iconScale, 1, scaleLerp);

            iconPivot.localScale = Vector3.one * scale;
        }
        
        private void TickDepthTowards(float target)
        {
            SetWindowDepth(Mathf.MoveTowards(windowDepthLerp, target, Time.deltaTime / windowLerpTime));
        }

        private void SetWindowDepth(float newDepth)
        {
            windowDepthLerp = newDepth;
            windowDepth = Mathf.SmoothStep(0, 1, windowDepthLerp);

            var s = windowScale.localScale;
            s.z = windowDepth;
            windowScale.localScale = s;
        }
    }
}