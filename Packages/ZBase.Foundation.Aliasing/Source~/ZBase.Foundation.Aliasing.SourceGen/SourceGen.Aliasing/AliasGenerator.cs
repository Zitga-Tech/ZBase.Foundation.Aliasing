using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;
using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.Aliasing
{
    [Generator]
    public class AliasGenerator : IIncrementalGenerator
    {
        public const string ATTRIBUTE_NAME = "Alias";
        public const string FULL_ATTRIBUTE_NAME = "global::ZBase.Foundation.Aliasing.AliasAttribute";
        public const string GENERATOR_NAME = nameof(AliasGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsSyntaxMatch,
                transform: GetSemanticSyntaxMatch
            ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combined = candidateProvider.Combine(compilationProvider).Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static bool IsSyntaxMatch(
              SyntaxNode syntaxNode
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not StructDeclarationSyntax structSyntax)
            {
                return false;
            }

            if (structSyntax.AttributeLists == null || structSyntax.AttributeLists.Count < 1)
            {
                return false;
            }

            foreach (var attribList in structSyntax.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    if (attrib.Name is IdentifierNameSyntax identifierNameSyntax
                        && identifierNameSyntax.Identifier.ValueText == ATTRIBUTE_NAME
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static StructDeclarationSyntax GetSemanticSyntaxMatch(
              GeneratorSyntaxContext syntaxContext
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxContext.Node is not StructDeclarationSyntax structSyntax)
            {
                return null;
            }

            var semanticModel = syntaxContext.SemanticModel;

            foreach (var attribList in structSyntax.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    var typeInfo = semanticModel.GetTypeInfo(attrib, token);
                    var fullName = typeInfo.Type.ToFullName();

                    if (fullName.StartsWith(FULL_ATTRIBUTE_NAME))
                    {
                        return structSyntax;
                    }
                }
            }

            return null;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , StructDeclarationSyntax candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = candidate.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = new AliasDeclaration(candidate, semanticModel, context.CancellationToken);

                if (declaration.IsValid == false)
                {
                    return;
                }

                var source = declaration.WriteCode();
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , candidate
                    , source
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, candidate)
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , candidate.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , candidate.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ALIAS_01"
                , "Alias Generator Error"
                , "This error indicates a bug in the Alias source generators. Error message: '{0}'."
                , "ZBase.Foundation.Aliasing.AliasAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
