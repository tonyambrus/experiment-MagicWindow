using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicWindow
{
    public class MagicWindow : MonoBehaviour
    {
        public class VisibilityCallback : MonoBehaviour
        {
            [NonSerialized]
            public MagicWindow target;

            void OnBecameVisible()
            {
                if (target)
                {
                    target.OnVisibilityChange(true);
                }
                else
                {
                    throw new Exception("No magic window target");
                }
            }

            void OnBecameInvisible()
            {
                if (target)
                {
                    target.OnVisibilityChange(false);
                }
                else
                {
                    throw new Exception("No magic window target");
                }
            }
        }

        public Renderer window;
        public Renderer hollow;
        public GameObject target;

        private int stencil;
        private List<Renderer> targetRenderers = new List<Renderer>();
        private MaterialPropertyBlock stencilBlock;
        private VisibilityCallback callback;

        private bool useStencil = true;
        private bool isStencilActive = true;
        private bool isVisible = false;

        private void Awake()
        {
            stencilBlock = new MaterialPropertyBlock();

            callback = window.gameObject.AddComponent<VisibilityCallback>();
            callback.target = this;
        }

        private void OnDestroy()
        {
            Destroy(callback);
        }

        public bool UseStencil
        {
            get => useStencil;
            set
            {
                if (useStencil != value)
                {
                    useStencil = value;
                    UpdateStencilActive();
                }
            }
        }

        private void UpdateStencilActive()
        {
            SetStencilActive(isVisible && useStencil);
        }

        void OnVisibilityChange(bool visible)
        {
            isVisible = visible;
            UpdateStencilActive();
        }

        void SetStencilActive(bool enable)
        {
            if (isStencilActive != enable)
            {
                isStencilActive = enable;

                if (enable && stencil == 0)
                {
                    stencil = StencilRegistry.Register();
                }
                else if (!enable && stencil != 0)
                {
                    StencilRegistry.Unregister(stencil);
                    stencil = 0;
                }

                stencilBlock.SetInt("_StencilMask", stencil);
                SetRendererStencil(window);
                SetRendererStencil(hollow);
                target.GetComponentsInChildren(includeInactive: true, targetRenderers);
                foreach (var r in targetRenderers)
                {
                    SetRendererStencil(r);
                }
            }
        }

        void SetRendererStencil(Renderer r)
        {
            // Note: Doesn't set correctly with SetPropertyBlock. Have to use material.SetInt
            //r.SetPropertyBlock(stencilBlock); 

            r.material.SetInt("_StencilMask", stencil);
        }
    }
}