using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.Aliasing
{
    public partial class AliasDeclaration
    {
        public const string FULL_ATTRIBUTE_NAME = "global::ZBase.Foundation.Aliasing.AliasAttribute";

        public StructDeclarationSyntax Syntax { get; private set; }

        public INamedTypeSymbol Symbol { get; private set; }

        public string TypeName { get; private set; }

        public string FullTypeName { get; private set; }

        public bool IsValid { get; private set; }

        public string FieldTypeName { get; private set; }

        public AliasOptions Options { get; private set; }

        public string ToStringFormat { get; private set; }

        public bool IsFieldDeclared { get; private set; }

        public bool IsReadOnly { get; private set; }

        public string FieldName { get; private set; }

        public OperatorOptions Operators { get; private set; }

        public string EqualityReturnTypeName { get; private set; }

        public string GreaterThanReturnTypeName { get; private set; }

        public string GreaterThanOrEqualReturnTypeName { get; private set; }

        public AliasDeclaration(StructDeclarationSyntax candidate, SemanticModel semanticModel, CancellationToken token)
        {
            Syntax = candidate;
            Symbol = semanticModel.GetDeclaredSymbol(candidate, token);
            TypeName = candidate.Identifier.ValueText;
            FullTypeName = Symbol.ToFullName();
            IsReadOnly = Symbol.IsReadOnly;

            ITypeSymbol fieldTypeSymbol = null;
            string fieldName = null;
            string toStringFormat = null;

            foreach (var attribList in candidate.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    var typeInfo = semanticModel.GetTypeInfo(attrib, token);
                    var fullName = typeInfo.Type.ToFullName();

                    if (fullName.StartsWith(FULL_ATTRIBUTE_NAME) == false)
                    {
                        continue;
                    }

                    if (attrib.ArgumentList == null || attrib.ArgumentList.Arguments.Count < 1)
                    {
                        continue;
                    }

                    for (var i = 0; i < attrib.ArgumentList.Arguments.Count; i++)
                    {
                        var arg = attrib.ArgumentList.Arguments[i];
                        var expr = arg.Expression;

                        if (i == 0) // Type type
                        {
                            if (expr is TypeOfExpressionSyntax typeOfExpr)
                            {
                                fieldTypeSymbol = semanticModel
                                    .GetSymbolInfo(typeOfExpr.Type).Symbol as ITypeSymbol
                                    ?? throw new Exception("Require type symbol.");

                                FieldTypeName = fieldTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            }
                            else
                            {
                                throw new Exception("Require [Alias] attribute and .ctor.");
                            }
                        }
                        else if (i == 1) // AliasOptions options
                        {
                            try
                            {
                                var parsed = Enum.ToObject(typeof(AliasOptions), semanticModel.GetConstantValue(expr).Value);
                                Options = (AliasOptions)parsed;
                            }
                            catch
                            {
                                Options = AliasOptions.Default;
                            }
                        }
                        else if (i == 2) // string fieldName
                        {
                            fieldName = semanticModel.GetConstantValue(expr).Value?.ToString();
                        }
                        else if (i == 3) // string toStringFormat
                        {
                            toStringFormat = semanticModel.GetConstantValue(expr).Value?.ToString();
                        }
                    }

                    if (fieldTypeSymbol != null)
                    {
                        IsValid = true;
                        break;
                    }
                }
            }

            if (IsValid == false)
            {
                return;
            }

            FieldName = string.IsNullOrWhiteSpace(fieldName) ? "value" : fieldName;
            ToStringFormat = string.IsNullOrWhiteSpace(toStringFormat) ? string.Empty : toStringFormat;

            var members = Symbol.GetMembers();
            var fieldTypeMembers = fieldTypeSymbol.GetMembers();

            Operators = OperatorOptions.None;
            EqualityReturnTypeName = "bool";
            GreaterThanReturnTypeName = "bool";
            GreaterThanOrEqualReturnTypeName = "bool";

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    if (field.Name == FieldName
                        && field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == FieldTypeName
                    )
                    {
                        IsFieldDeclared = true;
                        break;
                    }
                }
            }

            foreach (var member in fieldTypeMembers)
            {
                if (member is IMethodSymbol method)
                {
                    switch (method.Name)
                    {
                        case "op_Equality":
                        {
                            Operators |= OperatorOptions.Equality;

                            if (method.ReturnType.Name != "Boolean")
                            {
                                EqualityReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }

                        case "op_GreaterThan":
                        {
                            Operators |= OperatorOptions.GreaterThan;

                            if (method.ReturnType.Name != "Boolean")
                            {
                                GreaterThanReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }

                        case "op_GreaterThanOrEqual":
                        {
                            Operators |= OperatorOptions.GreaterThanOrEqual;

                            if (method.ReturnType.Name == "Boolean")
                            {
                                GreaterThanOrEqualReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }
                    }
                }
            }

            if (fieldTypeSymbol.TypeKind == TypeKind.Enum)
            {
                Operators |= OperatorOptions.Equality;
            }
        }

        public bool HasFlag(AliasOptions options)
            => Options.HasFlag(options);

        public bool HasOperator(OperatorOptions options)
            => Operators.HasFlag(options);
    }
}
