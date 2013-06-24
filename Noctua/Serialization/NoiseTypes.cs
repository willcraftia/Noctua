#region Using

using System;
using Musca;
using Pyxis;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public static class NoiseTypes
    {
        public static void SetTypeDefinitionNames(ModuleTypeRegistry typeRegistory)
        {
            if (typeRegistory == null) throw new ArgumentNullException("typeRegistory");

            // Face curves.
            typeRegistory.SetTypeDefinitionName(typeof(NoFadeCurve));
            typeRegistory.SetTypeDefinitionName(typeof(SCurve3));
            typeRegistory.SetTypeDefinitionName(typeof(SCurve5));

            // Gradient noises.
            typeRegistory.SetTypeDefinitionName(typeof(Perlin));
            typeRegistory.SetTypeDefinitionName(typeof(ClassicPerlin));
            typeRegistory.SetTypeDefinitionName(typeof(Simplex));

            // Perlin fractal function.
            typeRegistory.SetTypeDefinitionName(typeof(PerlinFractal));

            // Musgrave fractal functions.
            typeRegistory.SetTypeDefinitionName(typeof(Heterofractal));
            typeRegistory.SetTypeDefinitionName(typeof(HybridMultifractal));
            typeRegistory.SetTypeDefinitionName(typeof(Multifractal));
            typeRegistory.SetTypeDefinitionName(typeof(RidgedMultifractal));
            typeRegistory.SetTypeDefinitionName(typeof(SinFractal));
            typeRegistory.SetTypeDefinitionName(typeof(SumFractal));

            // Other fractal functions.
            typeRegistory.SetTypeDefinitionName(typeof(Billow));

            // Utility.
            typeRegistory.SetTypeDefinitionName(typeof(Const));

            // Controllers.
            typeRegistory.SetTypeDefinitionName(typeof(Add));
            typeRegistory.SetTypeDefinitionName(typeof(Cache));
            typeRegistory.SetTypeDefinitionName(typeof(Const));
            typeRegistory.SetTypeDefinitionName(typeof(Displace));
            typeRegistory.SetTypeDefinitionName(typeof(Multiply));
            typeRegistory.SetTypeDefinitionName(typeof(ScaleBias));
            typeRegistory.SetTypeDefinitionName(typeof(ScalePoint));
            typeRegistory.SetTypeDefinitionName(typeof(Select));

            // Custom controllers.
            typeRegistory.SetTypeDefinitionName(typeof(SelectTerrainDensity));
        }
    }
}
