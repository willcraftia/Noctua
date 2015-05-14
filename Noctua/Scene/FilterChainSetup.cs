#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public abstract class FilterChainSetup
    {
        public DeviceContext DeviceContext { get; private set; }

        public SceneManager Manager { get; private set; }

        public bool Enabled { get; set; }

        public bool Initialized { get; private set; }

        protected FilterChainSetup(DeviceContext deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            Enabled = true;
        }

        public virtual void Initialize(SceneManager manager)
        {
            Manager = manager;

            Initialized = true;
        }

        public abstract void Setup(FilterChain filterChain);
    }
}
