using System;
using Xunit;

namespace DotNext.VariantType
{
    public sealed class VariantTests : Assert
    {
        [Fact]
        public static void DynamicNull()
        {
            dynamic variant = default(Variant<string, Uri>);
            True(variant == null);
        }

        [Fact]
        public static void DynamicVariant()
        {
            Variant<string, Uri> variant = "Hello, world!";
            dynamic d = variant;
            Equal(5, d.IndexOf(','));
            variant = new Uri("http://contoso.com");
            d = variant;
            Equal("http", d.Scheme);
            Variant<string, Uri, Version> variant2 = variant;
            Equal(new Uri("http://contoso.com"), (Uri)variant2);
            Null((string)variant2);
            Null((Version)variant2);
        }

        [Fact]
        public static void Permutation()
        {
            var obj = new Variant<string, object>("Hello");
            var obj2 = obj.Permute();
            Equal<object>(obj, obj2);
            Equal("Hello", obj.First);
            Equal("Hello", obj.Second);
            Equal(obj.GetHashCode(), obj2.GetHashCode());
            Equal(obj.ToString(), obj2.ToString());
        }

        [Fact]
        public static void NullCheck()
        {
            Variant<string, object> obj = new object();
            False(obj.IsNull);
            obj = default;
            True(obj.IsNull);
        }

        [Fact]
        public static void Conversion()
        {
            Variant<string, object> obj = new object();
            var result = obj.Convert<string>(Func.Identity<string>().AsConverter(), value => value.ToString());
            True(result.IsPresent);
            NotNull(result.Value);
            obj = default;
            result = obj.Convert<string>(Func.Identity<string>().AsConverter(), value => value.ToString());
            False(result.IsPresent);
        }
    }
}