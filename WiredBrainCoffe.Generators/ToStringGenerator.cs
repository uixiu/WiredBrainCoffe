using System;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

 namespace WiredBrainCoffe.Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //IncrementalValuesProvider<ClassDeclarationSyntax>
         var classesSyntaxProvider = 
            context.SyntaxProvider.CreateSyntaxProvider(predicate: IsSyntaxTarget, transform: GetSemanticTargt)
                .Where(target => target is not null);
               
        context.RegisterSourceOutput( classesSyntaxProvider, Execute!);

        context.RegisterPostInitializationOutput( static (ctx) => PostInitializationOutput(ctx));
    }

  


    private static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
   {
         return node is ClassDeclarationSyntax classDeclarationSyntax
             && classDeclarationSyntax.AttributeLists.Count > 0;
   }
    
   private static ClassDeclarationSyntax? GetSemanticTargt(GeneratorSyntaxContext context, CancellationToken cancellationToken)
   {
        var classDeclarationSyntax =  (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var attributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("WiredBrainCoffe.Generators.GenerateToStringAttribute");
        if(classSymbol is null || attributeSymbol is null)
            return null;

        /*foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var attributeName = attributeSyntax.Name.ToString();
                if(attributeName is "GenerateToString" or "GenerateToStringAttribute")
                {
                    return classDeclarationSyntax;
                }
            }
        }*/

        if(classSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true))
        {
            return classDeclarationSyntax;
        }
        

        return null;
   }


    private static void Execute(SourceProductionContext productionContext, ClassDeclarationSyntax clsDeclarationSyntax)
    {
        if(clsDeclarationSyntax.Parent is not BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
            return;
        
        var namespaceName = namespaceDeclarationSyntax.Name.ToString();
        var className = clsDeclarationSyntax.Identifier.Text;
        var fileName = $"{namespaceName}.{className}.g.cs";

        var sourceBuilder = new StringBuilder();
        sourceBuilder.Append($$"""
                               namespace {{namespaceName}}
                               {
                                   partial class {{className}}
                                   {
                                       public override string ToString()
                                       {
                                         return $"
                               """);

        bool firstTime = true;
        foreach (var memberDeclarationSyntax in clsDeclarationSyntax.Members)
        {
            if(memberDeclarationSyntax is PropertyDeclarationSyntax propertyDeclarationSyntax
               && propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword) )
            {
                var propertyName = propertyDeclarationSyntax.Identifier.Text;
              
                var prefix = firstTime ? "" : "; ";
                firstTime = false;

                sourceBuilder.Append($"{prefix}{propertyName}:{{{propertyName}}}"); 
            }
        }

        sourceBuilder.Append($$"""
                               ";        
                                        }
                                    }
                               }                                                             
                               """);

        productionContext.AddSource(fileName, sourceBuilder.ToString());
    }

    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
       string source = """
                       namespace WiredBrainCoffe.Generators
                       {
                           public class GenerateToStringAttribute : System.Attribute { }
                       }
                       """;
            
        context.AddSource("WiredBrainCoffe.Generators.GenerateToStringAttribute.g.cs", source: source);
    }

}  
