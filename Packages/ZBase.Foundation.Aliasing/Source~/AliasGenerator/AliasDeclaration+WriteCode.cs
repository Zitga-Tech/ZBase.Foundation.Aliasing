using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.Aliasing
{
    partial class AliasDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

        public string WriteCode()
        {
            var @in = HasFlag(AliasOptions.IsReadOnlyRef) ? "in " : "";
            var implicitStr = HasFlag(AliasOptions.ImplicitOperator) ? "implicit" : "explicit";

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;

            p = p.IncreasedIndent();
            {
                p.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
                p.PrintLine($"[global::System.ComponentModel.TypeConverter(typeof({TypeName}TypeConverter))]");
                p.PrintLine($"partial struct {TypeName} : global::System.IEquatable<{FullTypeName}>");

                if (HasFlag(AliasOptions.Comparable))
                {
                    p = p.IncreasedIndent();
                    p.PrintLine($", global::System.IComparable<{FullTypeName}>");
                    p = p.DecreasedIndent();
                }

                p.OpenScope();
                {
                    if (IsFieldDeclared == false)
                    {
                        p.PrintBeginLine();
                        {
                            if (HasFlag(AliasOptions.ExposeValueAsPublicField))
                            {
                                p.Print("public ");
                            }

                            if (IsReadOnly)
                            {
                                p.Print("readonly ");
                            }

                            p.Print($"{FieldTypeName} {FieldName};");
                        }
                        p.PrintEndLine();
                    }

                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"public {FieldTypeName} AsPrimitive() => {FieldName};");

                    p.PrintEndLine();

                    p.PrintLine($"public {TypeName}({@in}{FieldTypeName} value) : this()");
                    p.OpenScope();
                    {
                        p.PrintLine($"this.{FieldName} = value;");

                        if (HasFlag(AliasOptions.Validate))
                        {
                            p.PrintLine("this.Validate();");
                        }
                    }
                    p.CloseScope();

                    if(HasFlag(AliasOptions.Validate))
                    {
                        p.PrintEndLine();
                        p.PrintLine("private partial void Validate();");
                    }

                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"public static {implicitStr} operator {FieldTypeName}({@in}{FullTypeName} value) => value.{FieldName};");

                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"public static {implicitStr} operator {FullTypeName}({@in}{FieldTypeName} value) => new {FullTypeName}(value);");

                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"public override int GetHashCode() => {FieldName}.GetHashCode();");

                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public override string ToString()");
                    p = p.IncreasedIndent();
                    if (string.IsNullOrEmpty(ToStringFormat))
                    {
                        p.PrintLine($"=> {FieldName}.ToString();");
                    }
                    else
                    {
                        p.PrintLine($"=> string.Format({ToStringFormat}, {FieldName});");
                    }
                    p = p.DecreasedIndent();

                    if (HasOperator(OperatorOptions.Equality) && EqualityReturnTypeName == "bool")
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public bool Equals({FullTypeName} other) => {FieldName} == other.{FieldName};");

                        p.PrintEndLine();
                        p.PrintLine($"public override bool Equals(object obj) => obj is {FullTypeName} other && {FieldName} == other.{FieldName};");
                    }
                    else
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public bool Equals({FullTypeName} other) => {FieldName}.Equals(other.{FieldName});");

                        p.PrintEndLine();
                        p.PrintLine($"public override bool Equals(object obj) => obj is {FullTypeName} other && {FieldName}.Equals(other.{FieldName});");
                    }

                    if (HasOperator(OperatorOptions.Equality))
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {EqualityReturnTypeName} operator ==({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName} == rhs.{FieldName};");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {EqualityReturnTypeName} operator !=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName} != rhs.{FieldName};");
                    }
                    else
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {EqualityReturnTypeName} operator ==({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName}.Equals(rhs.{FieldName});");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {EqualityReturnTypeName} operator !=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => !lhs.{FieldName}.Equals(rhs.{FieldName});");
                    }

                    if (HasFlag(AliasOptions.ArithmeticOperator))
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator +({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => new(({FieldTypeName})(lhs.{FieldName} + rhs.{FieldName}));");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator -({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => new(({FieldTypeName})(lhs.{FieldName} - rhs.{FieldName}));");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator *({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => new(({FieldTypeName})(lhs.{FieldName} * rhs.{FieldName}));");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator /({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => new(({FieldTypeName})(lhs.{FieldName} / rhs.{FieldName}));");
                    }

                    if (HasFlag(AliasOptions.ValueArithmeticOperator))
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator ++({@in}{FullTypeName} lhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} + 1)); }} }}");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator --({@in}{FullTypeName} lhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} - 1)); }} }}");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator +({@in}{FullTypeName} lhs, {@in}{FieldTypeName} rhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} + rhs.{FieldName})); }} }}");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator -({@in}{FullTypeName} lhs, {@in}{FieldTypeName} rhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} - rhs.{FieldName})); }} }}");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator *({@in}{FullTypeName} lhs, {@in}{FieldTypeName} rhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} * rhs.{FieldName})); }} }}");

                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public static {FullTypeName} operator /({@in}{FullTypeName} lhs, {@in}{FieldTypeName} rhs) {{ checked {{ return new(({FieldTypeName})(lhs.{FieldName} / rhs.{FieldName})); }} }}");
                    }

                    if (HasFlag(AliasOptions.Comparable))
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public int CompareTo({FullTypeName} other) => {FieldName}.CompareTo(other.{FieldName});");

                        if (HasFlag(AliasOptions.WithoutComparisonOperator) == false)
                        {
                            if (HasOperator(OperatorOptions.GreaterThan))
                            {
                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static {GreaterThanReturnTypeName} operator >({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs)  => lhs.{FieldName} > rhs.{FieldName};");

                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static {GreaterThanReturnTypeName} operator <({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName} < rhs.{FieldName};");
                            }
                            else
                            {
                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static bool operator >({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName}.CompareTo(rhs.{FieldName}) > 0;");

                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static bool operator <({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName}.CompareTo(rhs.{FieldName}) < 0;");
                            }

                            if (HasOperator(OperatorOptions.GreaterThanOrEqual))
                            {
                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static {GreaterThanOrEqualReturnTypeName} operator >=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName} >= rhs.{FieldName};");

                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static {GreaterThanOrEqualReturnTypeName} operator <=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName} <= rhs.{FieldName};");
                            }
                            else
                            {
                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static bool operator >=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName}.CompareTo(rhs.{FieldName}) >= 0;");

                                p.PrintEndLine();
                                p.PrintLine(AGGRESSIVE_INLINING);
                                p.PrintLine($"public static bool operator <=({@in}{FullTypeName} lhs, {@in}{FullTypeName} rhs) => lhs.{FieldName}.CompareTo(rhs.{FieldName}) <= 0;");
                            }
                        }
                    }

                    p.PrintEndLine();

                    p.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
                    p.PrintLine($"private class {TypeName}TypeConverter : global::System.ComponentModel.TypeConverter");
                    p.OpenScope();
                    {
                        p.PrintLine($"private static readonly global::System.Type s_wrapperType = typeof({FullTypeName});");
                        p.PrintLine($"private static readonly global::System.Type s_valueType = typeof({FieldTypeName});");

                        p.PrintEndLine();

                        p.PrintLine($"public override bool CanConvertFrom(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type sourceType)");
                        p.OpenScope();
                        {
                            p.PrintLine("if (sourceType == s_wrapperType || sourceType == s_valueType) return true;");
                            p.PrintLine("return base.CanConvertFrom(context, sourceType);");
                        }
                        p.CloseScope();

                        p.PrintEndLine();

                        p.PrintLine($"public override bool CanConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type destinationType)");
                        p.OpenScope();
                        {
                            p.PrintLine($"if (destinationType == s_wrapperType || destinationType == s_valueType) return true;");
                            p.PrintLine($"return base.CanConvertTo(context, destinationType);");
                        }
                        p.CloseScope();

                        p.PrintEndLine();

                        p.PrintLine($"public override object ConvertFrom(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value)");
                        p.OpenScope();
                        {
                            p.PrintLine("if (value != null)");
                            p.OpenScope();
                            {
                                p.PrintLine("var t = value.GetType();");
                                p.PrintLine($"if (t == typeof({FullTypeName})) return ({FullTypeName})value;");
                                p.PrintLine($"if (t == typeof({FieldTypeName})) return new {FullTypeName}(({FieldTypeName})value);");
                            }
                            p.CloseScope();

                            p.PrintLine("return base.ConvertFrom(context, culture, value);");
                        }
                        p.CloseScope();

                        p.PrintEndLine();

                        p.PrintLine($"public override object ConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value, global::System.Type destinationType)");
                        p.OpenScope();
                        {
                            p.PrintLine($"if (value is {FullTypeName} wrappedValue)");
                            p.OpenScope();
                            {
                                p.PrintLine("if (destinationType == s_wrapperType) return wrappedValue;");
                                p.PrintLine("if (destinationType == s_valueType) return wrappedValue.AsPrimitive();");
                            }
                            p.CloseScope();

                            p.PrintLine("return base.ConvertTo(context, culture, value, destinationType);");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();
            
            return p.Result;
        }
    }
}
