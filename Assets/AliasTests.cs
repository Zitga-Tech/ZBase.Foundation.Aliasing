using System.Runtime.InteropServices;
using ZBase.Foundation.Aliasing;

namespace AliasTests
{
    [Alias(typeof(int), AliasOptions.Default)]
    public partial struct AliasOfInt { }
    
    [Alias(typeof(int), AliasOptions.Default | AliasOptions.WithoutTypeConverter)]
    public partial struct AliasOfFloat { }

    public enum FruitType : byte
    {
        None,
        Apple,
        Banana,
    }

    [Alias(typeof(byte), AliasOptions.Default)]
    public readonly partial struct AliasOfFruitByte
    {
        [FieldOffset(0)]
        public readonly FruitType Fruit;

        public AliasOfFruitByte(FruitType value) : this()
        {
            Fruit = value;
        }

        public override string ToString()
        {
            return Fruit.ToString();
        }
    }
}