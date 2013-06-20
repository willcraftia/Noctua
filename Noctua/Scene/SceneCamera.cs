#region Using

using System;
using Libra;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    // TODO
    //
    // シーン オブジェクトとして実装すべきか？

    public class SceneCamera : BasicCamera
    {
        /// <summary>
        /// デフォルトの焦点距離。
        /// </summary>
        public const float DefaultFocusDistance = 3.0f;

        /// <summary>
        /// デフォルトの焦点範囲。
        /// </summary>
        public const float DefaultFocusRange = 200.0f;

        public string Name { get; private set; }

        public float FocusRange { get; private set; }

        public float FocusDistance { get; private set; }

        public SceneCamera(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            Name = name;

            View = Matrix.Identity;
            Projection = Matrix.Identity;
            FocusRange = DefaultFocusRange;
            FocusDistance = DefaultFocusDistance;
        }
    }
}
