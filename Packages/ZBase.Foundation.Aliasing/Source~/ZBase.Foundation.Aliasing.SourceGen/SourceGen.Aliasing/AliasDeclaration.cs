using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.Aliasing
{
    public partial class AliasDeclaration
    {
        public const string ALIAS_ATTRIBUTE = "global::ZBase.Foundation.Aliasing.AliasAttribute";

        public StructDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public ITypeSymbol FieldTypeSymbol { get; }

        public string TypeName { get; }

        public string FullTypeName { get; }

        public string FieldTypeName { get; }

        public string ToStringFormat { get; }

        public bool IsFieldDeclared { get; }

        public bool IsReadOnly { get; }

        public string FieldName { get; }

        public string EqualityReturnTypeName { get; }

        public string GreaterThanReturnTypeName { get; }

        public string GreaterThanOrEqualReturnTypeName { get; }

        public ImmutableArray<ISymbol> Members { get; }

        public ImmutableArray<INamedTypeSymbol> Interfaces { get; }

        public AliasDeclaration(
              StructDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            TypeName = syntax.Identifier.ValueText;
            FullTypeName = Symbol.ToFullName();
            IsReadOnly = Symbol.IsReadOnly;

            string fieldName = null;
            string toStringFormat = null;

            foreach (var attribList in syntax.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    if (attrib.Name.IsTypeNameCandidate("ZBase.Foundation.Aliasing", "Alias") == false
                        || attrib.ArgumentList == null
                        || attrib.ArgumentList.Arguments.Count < 1
                    )
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
                                FieldTypeSymbol = semanticModel.GetSymbolInfo(typeOfExpr.Type, token).Symbol as ITypeSymbol;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (i == 1) // string fieldName
                        {
                            fieldName = semanticModel.GetConstantValue(expr).Value?.ToString();
                        }
                        else if (i == 2) // string toStringFormat
                        {
                            toStringFormat = semanticModel.GetConstantValue(expr).Value?.ToString();
                        }
                    }

                    if (FieldTypeSymbol != null)
                    {
                        break;
                    }
                }
            }

            if (FieldTypeSymbol == null)
            {
                return;
            }

            FieldTypeName = FieldTypeSymbol.ToFullName();
            FieldName = string.IsNullOrWhiteSpace(fieldName) ? "value" : fieldName;
            ToStringFormat = string.IsNullOrWhiteSpace(toStringFormat) ? string.Empty : toStringFormat;

            var members = Symbol.GetMembers();
            var fieldTypeMembers = FieldTypeSymbol.GetMembers();

            EqualityReturnTypeName = "bool";
            GreaterThanReturnTypeName = "bool";
            GreaterThanOrEqualReturnTypeName = "bool";

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    if (field.Name == FieldName && field.Type.ToFullName() == FieldTypeName)
                    {
                        IsFieldDeclared = true;
                        break;
                    }
                }
            }

            using var memberArrayBuilder = ImmutableArrayBuilder<ISymbol>.Rent();
            var memberStrings = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in fieldTypeMembers)
            {
                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (member is IMethodSymbol method)
                {
                    if (method.MethodKind is (MethodKind.PropertyGet or MethodKind.PropertySet))
                    {
                        continue;
                    }

                    switch (method.Name)
                    {
                        case "op_Equality":
                        {
                            if (method.ReturnType.Name != "Boolean")
                            {
                                EqualityReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }

                        case "op_GreaterThan":
                        {
                            if (method.ReturnType.Name != "Boolean")
                            {
                                GreaterThanReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }

                        case "op_GreaterThanOrEqual":
                        {
                            if (method.ReturnType.Name == "Boolean")
                            {
                                GreaterThanOrEqualReturnTypeName = method.ReturnType.ToString();
                            }

                            break;
                        }
                    }
                }

                memberArrayBuilder.Add(member);
                memberStrings.Add(member.ToDisplayString(SymbolExtensions.MemberFormat));
            }

            Members = memberArrayBuilder.ToImmutable();

            using var interfaceArrayBuilder = ImmutableArrayBuilder<INamedTypeSymbol>.Rent();

            foreach (var @interface in FieldTypeSymbol.AllInterfaces)
            {
                var valid = true;

                foreach (var member in @interface.GetMembers())
                {
                    if (memberStrings.Contains(member.ToDisplayString(SymbolExtensions.MemberFormat)) == false)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid == false)
                {
                    continue;
                }

                interfaceArrayBuilder.Add(@interface);
            }

            Interfaces = interfaceArrayBuilder.ToImmutable();
        }
    }
}
