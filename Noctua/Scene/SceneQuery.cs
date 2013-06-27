#region Using

using System;

#endregion

namespace Noctua.Scene
{
    public abstract class SceneQuery
    {
        public SceneManager Manager { get; private set; }

        protected SceneQuery(SceneManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            Manager = manager;
        }

        public abstract void Execute(SceneQueryListener listener);
    }
}
