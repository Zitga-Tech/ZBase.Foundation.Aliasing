using System;
using ZBase.Foundation.Aliasing;

namespace AliasTest
{
    [Alias(typeof(int), AliasOptions.Default | AliasOptions.ValueArithmeticOperator)]
    public partial struct AliasOfInt { }

    [Alias(typeof(short), AliasOptions.Default | AliasOptions.ArithmeticOperator | AliasOptions.ValueArithmeticOperator)]
    public partial struct AliasOfShort { }
}

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

    /// <summary>
    /// Add to structs that want to be an alias of another <see cref="Type"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class AliasAttribute : Attribute
    {
        /// <summary>
        /// Type of the underlying value of this struct
        /// </summary>
        public Type Type { get; }

        public AliasOptions Options { get; }

        /// <summary>
        /// Name of the underlying field
        /// </summary>
        /// <remarks>
        /// The default name is <c>value</c>
        /// </remarks>
        public string FieldName { get; }

        public string Format { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type of the underlying value of this struct</param>
        /// <param name="options"></param>
        /// <param name="fieldName">Name of the underlying field</param>
        /// <param name="toStringFormat"></param>
        public AliasAttribute(
              Type type
            , AliasOptions options = AliasOptions.None
            , string fieldName = "value"
            , string toStringFormat = null
        )
        {
            this.Type = type;
            this.Options = options;
            this.FieldName = fieldName;
            this.Format = toStringFormat;
        }
    }
}