#region Using

using System;
using Libra;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    public sealed class SceneSettings
    {
        public const float DefaultFogStart = 0.9f;

        public const float DefaultFogEnd = 1.0f;

        public const float DefaultSecondsPerDay = 10.0f;

        public const float DefaultFixedSecondsPerDay = 0.0f;

        public static Vector3 DefaultMidnightSunDirection
        {
            get
            {
                var direction = new Vector3(0, -1, 1);
                direction.Normalize();
                return direction;
            }
        }

        public static Vector3 DefaultSunlightDiffuseColor
        {
            get { return Vector3.One; }
        }

        public static Vector3 DefaultSunlightSpecularColor
        {
            get { return Vector3.Zero; }
        }

        public static Vector3 DefaultMidnightMoonDirection
        {
            get
            {
                var direction = new Vector3(0, 1, 1);
                direction.Normalize();
                return direction;
            }
        }

        public static Vector3 DefaultMoonlightDiffuseColor
        {
            get { return Vector3.One; }
        }

        public static Vector3 DefaultMoonlightSpecularColor
        {
            get { return Vector3.Zero; }
        }

        public static Vector3 DefaultShadowColor
        {
            get { return Vector3.Zero; }
        }

        Vector3 midnightSunDirection = DefaultMidnightSunDirection;

        Vector3 midnightMoonDirection = DefaultMidnightMoonDirection;

        Vector3 shadowColor = DefaultShadowColor;

        float fogStart = DefaultFogStart;

        float fogEnd = DefaultFogEnd;

        float secondsPerDay = DefaultSecondsPerDay;

        float fixedSecondsPerDay = DefaultFixedSecondsPerDay;

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

        public TimeColorCollection SkyColors { get; private set; }

        public TimeColorCollection AmbientLightColors { get; private set; }

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

        public float ElapsedSecondsPerDay { get; private set; }

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

        public Vector3 CurrentAmbientLightColor { get; private set; }

        public SceneSettings()
        {
            Sunlight = new DirectionalLight("Sun");
            Sunlight.Direction = -DefaultMidnightSunDirection;
            Sunlight.DiffuseColor = DefaultSunlightDiffuseColor;
            Sunlight.SpecularColor = DefaultSunlightSpecularColor;

            Moonlight = new DirectionalLight("Moon");
            Moonlight.Direction = -DefaultMidnightMoonDirection;
            Moonlight.DiffuseColor = DefaultMoonlightDiffuseColor;
            Moonlight.SpecularColor = DefaultMoonlightSpecularColor;

            SkyColors = new TimeColorCollection();
            AmbientLightColors = new TimeColorCollection();
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

            //----------------------------------------------------------------
            // 太陽と月

            UpdateSun();
            UpdateMoon();

            //----------------------------------------------------------------
            // 環境光

            UpdateAmbientLightColor();

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

            initialized = true;
        }

        void UpdateSun()
        {
            // 0 時での太陽の位置を基点に、設定された軸の周囲で太陽を回転。

            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref sunRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightSunDirection, ref transform, out sunDirection);
            sunDirection.Normalize();

            Sunlight.Direction = -sunDirection;
        }

        void UpdateMoon()
        {
            // 0 時での月の位置を基点に、設定された軸の周囲で月を回転。

            var angle = (ElapsedSecondsPerDay / secondsPerDay) * MathHelper.TwoPi;
            Matrix transform;
            Matrix.CreateFromAxisAngle(ref moonRotationAxis, angle, out transform);
            Vector3.Transform(ref midnightMoonDirection, ref transform, out moonDirection);
            moonDirection.Normalize();

            Moonlight.Direction = -moonDirection;
        }

        void UpdateAmbientLightColor()
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = ElapsedSecondsPerDay / SecondsPerDay;

            CurrentAmbientLightColor = AmbientLightColors.GetColor(elapsed);
        }

        void UpdateSkyColor()
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = ElapsedSecondsPerDay / SecondsPerDay;

            CurrentSkyColor = SkyColors.GetColor(elapsed);
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
