using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.Aliasing
{
    partial class AliasDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"ZBase.Foundation.Aliasing.AliasGenerator\", \"2.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public string WriteCode()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            var memberSet = new HashSet<string>(StringComparer.Ordinal);

            p = p.IncreasedIndent();
            {
                p.PrintLine($"[global::System.ComponentModel.TypeConverter(typeof({TypeName}TypeConverter))]");
                p.PrintLine($"partial struct {TypeName}");
                WriteInterfaces(ref p);
                p.OpenScope();
                {
                    WriteBackingField(ref p);
                    WriteConstructor(ref p);
                    WriteMembers(ref p);
                    WriteTypeConverter(ref p);
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();
            
            return p.Result;
        }

        private void WriteInterfaces(ref Printer p)
        {
            p = p.IncreasedIndent();

            for (var i = 0; i < Interfaces.Length; i++)
            {
                var comma = i == 0 ? ":" : ",";
                var @interface = Interfaces[i];
                p.PrintLine($"//{comma} {@interface.ToFullName()}");

                if (@interface.IsGenericType)
                {
                    p.PrintBeginLine($"//, {@interface.ToDisplayString(SymbolExtensions.QualifiedFormatNoGeneric)}<");

                    var args = @interface.TypeArguments;

                    for (var k = 0; k < args.Length; k++)
                    {
                        var argComma = k < args.Length - 1 ? ", " : "";
                        var arg = args[k];

                        if (arg.ToFullName() == FieldTypeName)
                        {
                            p.Print($"{argComma}{FullTypeName}");
                        }
                        else
                        {
                            p.Print($"{argComma}{arg.ToFullName()}");
                        }
                    }

                    p.PrintEndLine(">");
                }
            }

            p = p.DecreasedIndent();
        }

        private void WriteBackingField(ref Printer p)
        {
            if (IsFieldDeclared)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public ");

            if (IsReadOnly)
            {
                p.Print("readonly ");
            }

            p.PrintEndLine($"{FieldTypeName} {FieldName};");
            p.PrintEndLine();
        }

        private void WriteConstructor(ref Printer p)
        {
            p.PrintLine($"public {TypeName}({FieldTypeName} value)");
            p.OpenScope();
            {
                p.PrintLine($"this.{FieldName} = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteMembers(ref Printer p)
        {
            foreach (var member in Members)
            {
                if (member is IFieldSymbol field)
                {
                    WriteField(ref p, field);
                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    WriteProperty(ref p, property);
                    continue;
                }

                if (member is IMethodSymbol method)
                {
                    WriteMethod(ref p, method);
                    continue;
                }
            }
        }

        private void WriteField(ref Printer p, IFieldSymbol field)
        {
            var typeName = field.Type.ToFullName();

            if (field.IsConst)
            {
                if (typeName == FieldTypeName)
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintLine($"public static readonly {FullTypeName} {field.Name} = new {FullTypeName}({typeName}.{field.Name});");
                }
                else
                {
                    p.PrintLine(GENERATED_CODE);
                    p.PrintLine($"public const {typeName} {field.Name} = {typeName}.{field.Name};");
                }
            }
            else if (field.IsStatic)
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine("public static ");

                if (field.IsReadOnly)
                {
                    p.Print("readonly ");
                }

                if (typeName == FieldTypeName)
                {
                    p.Print(FullTypeName);
                }
                else
                {
                    p.Print(typeName);
                }

                p.Print($" {field.Name} = {typeName}.{field.Name};").PrintEndLine();
            }

            p.PrintEndLine();
        }

        private void WriteProperty(ref Printer p, IPropertySymbol property)
        {
            var typeName = property.Type.ToFullName();

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ");

            if (property.IsStatic)
            {
                p.Print("static ");
            }

            if (property.RefKind == RefKind.Ref)
            {
                p.Print("ref ");
            }
            else if (property.RefKind == RefKind.RefReadOnly)
            {
                p.Print("ref readonly ");
            }

            if (typeName == FieldTypeName)
            {
                p.Print(FullTypeName);
            }
            else
            {
                p.Print(typeName);
            }

            p.Print($" {property.Name}").PrintEndLine();

            if (property.IsStatic)
            {
                WritePropertyBody(ref p, property, typeName);
            }
            else
            {
                WritePropertyBody(ref p, property, FieldName);
            }

            p.PrintEndLine();

            static void WritePropertyBody(ref Printer p, IPropertySymbol property, string accessor)
            {
                p.OpenScope();
                {
                    if (property.GetMethod != null)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("get => ");

                        if (property.RefKind is (RefKind.Ref or RefKind.RefReadOnly))
                        {
                            p.Print("ref ");
                        }

                        p.Print($"{accessor}.{property.Name};").PrintEndLine();
                    }

                    if (property.SetMethod != null)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"set => {accessor}.{property.Name} = value;");
                    }
                }
                p.CloseScope();
            }

            p.PrintEndLine();
        }

        private void WriteMethod(ref Printer p, IMethodSymbol method)
        {
            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ");

            if (method.IsStatic)
            {
                p.Print("static ");
            }

            if (method.IsReadOnly)
            {
                p.Print("readonly ");
            }
            else if (method.RefKind == RefKind.Ref)
            {
                p.Print("ref ");
            }
            else if (method.RefKind == RefKind.RefReadOnly)
            {
                p.Print("ref readonly ");
            }

            if (method.ReturnsVoid)
            {
                p.Print("void");
            }
            else
            {
                p.Print(method.ReturnType.ToFullName());
            }

            p.Print($" {method.Name}(");

            p.PrintEndLine(")");
            p.OpenScope();
            {

            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTypeConverter(ref Printer p)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"private class {TypeName}TypeConverter : global::System.ComponentModel.TypeConverter");
            p.OpenScope();
            {
                p.PrintLine($"private static readonly global::System.Type s_wrapperType = typeof({FullTypeName});");
                p.PrintLine($"private static readonly global::System.Type s_valueType = typeof({FieldTypeName});");

                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public override bool CanConvertFrom(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type sourceType)");
                p.OpenScope();
                {
                    p.PrintLine("if (sourceType == s_wrapperType || sourceType == s_valueType) return true;");
                    p.PrintLine("return base.CanConvertFrom(context, sourceType);");
                }
                p.CloseScope();

                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public override bool CanConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Type destinationType)");
                p.OpenScope();
                {
                    p.PrintLine($"if (destinationType == s_wrapperType || destinationType == s_valueType) return true;");
                    p.PrintLine($"return base.CanConvertTo(context, destinationType);");
                }
                p.CloseScope();

                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
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

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public override object ConvertTo(global::System.ComponentModel.ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value, global::System.Type destinationType)");
                p.OpenScope();
                {
                    p.PrintLine($"if (value is {FullTypeName} wrappedValue)");
                    p.OpenScope();
                    {
                        p.PrintLine("if (destinationType == s_wrapperType) return wrappedValue;");
                        p.PrintLine($"if (destinationType == s_valueType) return wrappedValue.{FieldName};");
                    }
                    p.CloseScope();

                    p.PrintLine("return base.ConvertTo(context, culture, value, destinationType);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
