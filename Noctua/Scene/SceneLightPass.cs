#region Using

using System;
using Libra.Graphics;

#endregion

namespace Noctua.Scene
{
    public abstract class SceneLightPass
    {
        public SceneManager Manager { get; private set; }

        public DeviceContext DeviceContext { get; private set; }

        public bool Enabled { get; set; }

        public bool Initialized { get; private set; }

        protected SceneLightPass(DeviceContext deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            Enabled = true;
        }

        internal void Initialize(SceneManager manager)
        {
            Manager = manager;

            Initialize();

            Initialized = true;
        }

        protected virtual void Initialize() { }

        public abstract void Draw();
    }
}
