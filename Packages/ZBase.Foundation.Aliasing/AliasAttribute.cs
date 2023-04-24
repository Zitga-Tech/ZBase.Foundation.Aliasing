// Based on:
// https://github.com/Cysharp/UnitGenerator

using System;

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