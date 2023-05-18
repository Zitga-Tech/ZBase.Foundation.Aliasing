// Based on:
// https://github.com/Cysharp/UnitGenerator

using System;

namespace ZBase.Foundation.Aliasing
{
    [Flags]
    public enum AliasOptions
    {
        None                                 = 0,
        ImplicitOperator                     = 1 << 0,
        ArithmeticOperator                   = 1 << 1,
        ValueArithmeticOperator              = 1 << 2,
        Comparable                           = 1 << 3,
        WithoutComparisonOperator            = 1 << 4,
        ExposeValueAsPublicField             = 1 << 5,
        IsReadOnlyRef                        = 1 << 6,
        Validate                             = 1 << 7,

        Default                              = ImplicitOperator | ExposeValueAsPublicField,
        DefaultReadOnlyRef                   = Default | IsReadOnlyRef,
    }
}