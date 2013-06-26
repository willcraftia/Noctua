#region Using

using System;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Scene
{
    public abstract class SceneObject
    {
        /// <summary>
        /// 位置 (ワールド空間)。
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// オブジェクトのメッシュ全体を含む境界ボックス (ワールド空間)。
        /// </summary>
        public BoundingBox Box;

        /// <summary>
        /// オブジェクトのメッシュ全体を含む境界球 (ワールド空間)。
        /// </summary>
        public BoundingSphere Sphere;

        /// <summary>
        /// オブジェクト名を取得します。
        /// </summary>
        /// <remarks>
        /// オブジェクト名はノード内で一意です。
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// 親ノードを取得します。
        /// </summary>
        public SceneNode Parent { get; internal set; }

        /// <summary>
        /// 可視か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (可視の場合)、false (それ以外の場合)。
        /// </value>
        public bool Visible { get; set; }

        /// <summary>
        /// 半透明か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </value>
        public bool Translucent { get; set; }

        /// <summary>
        /// オクルージョン カリングされたか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (オクルージョン カリングされた場合)、false (それ以外の場合)。
        /// </value>
        public bool Occluded { get; protected set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <remarks>
        /// オブジェクト名はノード内で一意です。
        /// </remarks>
        /// <param name="name">オブジェクト名。</param>
        protected SceneObject(string name)
        {
            Name = name;
            Visible = true;
        }

        /// <summary>
        /// オクルージョン クエリを用いる場合、サブクラスでオーバライドし、
        /// オクルージョン クエリを適切に更新するように実装します。
        /// このメソッドは、各 Draw メソッドの前に呼び出されます。
        /// </summary>
        public virtual void UpdateOcclusion() { }

        /// <summary>
        /// オブジェクトを描画します。
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// シーン マネージャが指定するエフェクトを用いて描画します。
        /// </summary>
        /// <param name="effect">エフェクト。</param>
        public virtual void Draw(IEffect effect) { }
    }
}
