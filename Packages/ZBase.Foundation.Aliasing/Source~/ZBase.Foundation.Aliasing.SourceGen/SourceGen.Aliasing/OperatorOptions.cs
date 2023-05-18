// Based on:
// https://github.com/Cysharp/UnitGenerator

using System;

namespace ZBase.Foundation.Aliasing
{
    [Flags]
    public enum OperatorOptions
    {
        None               = 0,
        Equality           = 1 << 0,
        GreaterThan        = 1 << 1,
        GreaterThanOrEqual = 1 << 2,
    }
}
