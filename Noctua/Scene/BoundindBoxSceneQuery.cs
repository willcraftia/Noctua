#region Using

using System;
using Libra;

#endregion

namespace Noctua.Scene
{
    public sealed class BoundindBoxSceneQuery : SceneQuery
    {
        BoundingBox box = BoundingBox.Empty;

        public BoundingBox Box
        {
            get { return box; }
            set { box = value; }
        }

        public BoundindBoxSceneQuery(SceneManager manager)
            : base(manager)
        {
        }

        public override void Execute(SceneQueryListener listener)
        {
            throw new NotImplementedException();
        }
    }
}
