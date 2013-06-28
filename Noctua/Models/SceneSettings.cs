#region Using

using System;
using Libra;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    public sealed class SceneSettings
    {
        Vector3 midnightSunDirection = new Vector3(0, -1, 1);

        Vector3 midnightMoonDirection = new Vector3(0, 1, 1);

        Vector3 shadowColor = Vector3.Zero;

        Vector3 skyColor = Color.CornflowerBlue.ToVector3();

        float fogStart = 0.5f;

        float fogEnd = 1.0f;

        float secondsPerDay = 10.0f;

        float fixedSecondsPerDay = 0.0f;

        Vector3 sunRotationAxis;

        Vector3 moonRotationAxis;

        float halfDaySeconds;

        float inverseHalfDaySeconds;

        bool initialized;

        Vector3 sunDirection;

        Vector3 moonDirection;

        public Vector3 MidnightSunDirection
        {
            get { return midnightSunDirection; }
            set
            {
                if (value.LengthSquared() == 0) throw new ArgumentException("value");

                midnightSunDirection = value;
                midnightSunDirection.Normalize();
            }
        }

        public Vector3 MidnightMoonDirection
        {
            get { return midnightMoonDirection; }
            set
            {
                if (value.LengthSquared() == 0) throw new ArgumentException("value");

                midnightMoonDirection = value;
                midnightMoonDirection.Normalize();
            }
        }

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public bool FogEnabled { get; set; }

        // 遠クリップ面距離に対する割合
        public float FogStart
        {
            get { return fogStart; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                fogStart = value;
            }
        }

        // 遠クリップ面距離に対する割合
        public float FogEnd
        {
            get { return fogEnd; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                fogEnd = value;
            }
        }

        public Vector3 SkyColor
        {
            get { return skyColor; }
            set { skyColor = value; }
        }

        // SunlightEnabled や MoonlightEnabled は、
        // シーン設定の更新処理の制御を行う。
        // false の場合、対応するディレクショナル ライトを更新しない。
        // true の場合、対応するディレクショナル ライトを更新する。
        // ただし、実際にそれらのディレクショナル ライトが有効であるか否かは、
        // ディレクショナル ライトの Enabled プロパティに委ねられる。
        //
        // なお、ディレクショナル ライトとしての太陽光と、
        // 大気光としての意味を含む太陽光の使い分けに注意すること。
        // 例えば、夜間にディレクショナル ライトとしての太陽光を無効化しても、
        // 大気光を得るために太陽光の拡散反射光に直接アクセスすることがある。

        public bool SunlightEnabled { get; set; }

        public bool MoonlightEnabled { get; set; }

        public TimeColorCollection SunlightDiffuseColors { get; private set; }

        public TimeColorCollection MoonlightDiffuseColors { get; private set; }

        public Vector3 SunRotationAxis
        {
            get { return sunRotationAxis; }
        }

        public Vector3 MoonRotationAxis
        {
            get { return moonRotationAxis; }
        }

        public DirectionalLight Sunlight { get; private set; }

        public DirectionalLight Moonlight { get; private set; }

        public float SecondsPerDay
        {
            get { return secondsPerDay; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                secondsPerDay = value;
            }
        }

        public bool TimeStopped { get; set; }

        public float FixedSecondsPerDay
        {
            get { return fixedSecondsPerDay; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                fixedSecondsPerDay = value;
            }
        }

        /// <summary>
        /// 0 時からの経過時間 (秒) を取得します。
        /// </summary>
        public float ElapsedSecondsPerDay { get; private set; }

        /// <summary>
        /// 時間 (0 を 0 時、1 を 24 時とした時間) を取得します。
        /// </summary>
        public float Time { get; private set; }

        public Vector3 SunDirection
        {
            get { return sunDirection; }
        }

        public Vector3 MoonDirection
        {
            get { return moonDirection; }
        }

        public bool SunAboveHorizon
        {
            get { return 0 <= sunDirection.Y; }
        }

        public bool MoonAboveHorizon
        {
            get { return 0 <= moonDirection.Y; }
        }

        public Vector3 CurrentSkyColor { get; private set; }

        public SceneSettings()
        {
            midnightSunDirection.Normalize();
            midnightMoonDirection.Normalize();

            Sunlight = new DirectionalLight("Sun");
            Sunlight.Direction = -midnightSunDirection;
            Sunlight.DiffuseColor = Vector3.Zero;
            Sunlight.SpecularColor = Vector3.Zero;

            Moonlight = new DirectionalLight("Moon");
            Moonlight.Direction = -midnightMoonDirection;
            Moonlight.DiffuseColor = Vector3.One;
            Moonlight.SpecularColor = Vector3.Zero;

            SunlightDiffuseColors = new TimeColorCollection();
            MoonlightDiffuseColors = new TimeColorCollection();
        }

        public void Update(GameTime gameTime)
        {
            if (!initialized)
                Initialize();

            //----------------------------------------------------------------
            // 0 時からの経過時間 (ゲーム内での一日の経過時間)

            if (!TimeStopped)
            {
                ElapsedSecondsPerDay = (float) gameTime.TotalGameTime.TotalSeconds % secondsPerDay;
            }
            else
            {
                ElapsedSecondsPerDay = fixedSecondsPerDay;
            }

            Time = ElapsedSecondsPerDay / secondsPerDay;

            //----------------------------------------------------------------
            // 太陽と月

            if (SunlightEnabled)
                UpdateSun();
            
            if (MoonlightEnabled)
                UpdateMoon();

            //----------------------------------------------------------------
            // 空の色

            UpdateSkyColor();
        }

        void Initialize()
        {
            InitializeSunRotationAxis();
            InitializeMoonRotationAxis();

            halfDaySeconds = secondsPerDay * 0.5f;
            inverseHalfDaySeconds = 1 / halfDaySeconds;

            Sunlight.Enabled = SunlightEnabled;
            Moonlight.Enabled = MoonlightEnabled;

            initialized = true;
        }

        void UpdateSun()
        {
            // 0 時での太陽の位置を基点に、設定された軸の周囲で太陽を回転。

            var angle = Time * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref sunRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightSunDirection, ref transform, out sunDirection);
            sunDirection.Normalize();

            Sunlight.Direction = -sunDirection;
            Sunlight.DiffuseColor = SunlightDiffuseColors.GetColor(Time);
        }

        void UpdateMoon()
        {
            // 0 時での月の位置を基点に、設定された軸の周囲で月を回転。

            var angle = Time * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref moonRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightMoonDirection, ref transform, out moonDirection);
            moonDirection.Normalize();

            Moonlight.Direction = -moonDirection;
            Moonlight.DiffuseColor = MoonlightDiffuseColors.GetColor(Time);
        }

        void UpdateSkyColor()
        {
            if (SunlightEnabled)
            {
                CurrentSkyColor = SkyColor * Sunlight.DiffuseColor;
            }
            else
            {
                CurrentSkyColor = SkyColor;
            }
        }

        void InitializeSunRotationAxis()
        {
            // 0 時での太陽の位置から回転軸を算出。
            var left = Vector3.Cross(midnightSunDirection, Vector3.Up);
            sunRotationAxis = Vector3.Cross(left, midnightSunDirection);
        }

        void InitializeMoonRotationAxis()
        {
            // 0 時での月の位置から回転軸を算出。
            var left = Vector3.Cross(midnightMoonDirection, Vector3.Up);
            moonRotationAxis = Vector3.Cross(left, midnightMoonDirection);
        }
    }
}
