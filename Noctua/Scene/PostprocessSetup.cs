#region Using

using System;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public abstract class PostprocessSetup
    {
        public SceneManager Manager { get; private set; }

        public bool Enabled { get; set; }

        public bool Initialized { get; private set; }

        protected PostprocessSetup()
        {
            Enabled = true;
        }

        public virtual void Initialize(SceneManager manager)
        {
            Manager = manager;

            Initialized = true;
        }

        public abstract void Setup(Postprocess postprocess);
    }
}
