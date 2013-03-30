#region Using

using System;

#endregion

namespace Carina
{
    [Flags]
    public enum AppendNumberOptions
    {
        // The normal format.
        None = 0,

        // Appends "+" to use a positive value.
        PositiveSign = 1,

        // Appends "," every 3 digits.
        NumberGroup = 2,
    }
}
