using System;
using ZBase.Foundation.Aliasing;

namespace AliasTest
{
    [Alias(typeof(int))]
    public partial struct AliasOfInt
    {
    }

    [Alias(typeof(AttributeTargets))]
    public partial struct AliasOfEnum
    {

    }
}

namespace ZBase.Foundation.Aliasing
{
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
        /// <param name="fieldName">Name of the underlying field</param>
        /// <param name="toStringFormat"></param>
        public AliasAttribute(
              Type type
            , string fieldName = "value"
            , string toStringFormat = null
        )
        {
            this.Type = type;
            this.FieldName = fieldName;
            this.Format = toStringFormat;
        }
    }
}