using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliArgumentInfo : CliSymbolInfo, IEquatable<CliArgumentInfo>
    {
        public static readonly string AttributeFullName = typeof(CliArgumentAttribute).FullName;
        public const string AttributeNameProperty = nameof(CliArgumentAttribute.Name);
        public const string AttributeRequiredProperty = nameof(CliArgumentAttribute.Required);
        public const string AttributeArityProperty = nameof(CliArgumentAttribute.Arity);
        public const string AttributeAllowedValuesProperty = nameof(CliArgumentAttribute.AllowedValues);
        public const string AttributeAllowExistingProperty = nameof(CliArgumentAttribute.AllowExisting);
        public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Argument").Append("Argument").ToArray();
        public const string ArgumentClassName = "Argument";
        public const string ArgumentClassNamespace = "System.CommandLine";
        public const string ArgumentArityClassName = "ArgumentArity";
        public const string DiagnosticName = "CLI argument";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            { nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };

        public CliArgumentInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
         : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            ParseInfo = new CliArgumentParseInfo(Symbol, syntaxNode, semanticModel, this);

            Analyze();

            if (HasProblem)
                return;

            AttributeArguments = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
                .ToImmutableDictionary(pair => pair.Key, pair => pair.Value);

            if (AttributeArguments.TryGetValue(AttributeRequiredProperty, out var requiredTypedConstant)
                && requiredTypedConstant.Value != null)
                Required = (bool)requiredTypedConstant.Value;
            else
                Required = (SyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax && propertyDeclarationSyntax.Initializer != null)
                    ? propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.NullKeyword)
                      || propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                    : Symbol.Type.IsReferenceType || Symbol.IsRequired;
        }

        public CliArgumentInfo(GeneratorAttributeSyntaxContext attributeSyntaxContext)
            : this(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null)
        {
        }

        public new IPropertySymbol Symbol { get; }

        public ImmutableDictionary<string, TypedConstant> AttributeArguments { get; }

        public CliCommandInfo Parent { get; }

        public bool Required { get; }

        public CliArgumentParseInfo ParseInfo { get; set; }

        private void Analyze()
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningPropertyNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.GetMethod == null
                    || (Symbol.GetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.GetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicGetter, DiagnosticName);

                if (Symbol.SetMethod == null
                    || (Symbol.SetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.SetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicSetter, DiagnosticName);
            }
        }

        public override void ReportDiagnostics(SourceProductionContext sourceProductionContext)
        {
            base.ReportDiagnostics(sourceProductionContext); //self

            ParseInfo.ReportDiagnostics(sourceProductionContext);
        }
        
        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varDefaultValue)
        {
            var argumentName = AttributeArguments.TryGetValue(AttributeNameProperty, out var nameTypedConstant)
                                        && !string.IsNullOrWhiteSpace(nameTypedConstant.Value?.ToString())
                ? nameTypedConstant.Value.ToString().Trim()
                : Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention);

            sb.AppendLine($"// Argument for '{Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {ArgumentClassNamespace}.{ArgumentClassName}<{Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"\"{argumentName}\",");
                ParseInfo.AppendCSharpCallString(sb);
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case AttributeNameProperty:
                        case AttributeAllowedValuesProperty:
                        case AttributeRequiredProperty:
                        case AttributeAllowExistingProperty:
                            continue;
                        case AttributeArityProperty:
                            var arity = kvp.Value.ToCSharpString().Split('.').Last();
                            sb.AppendLine($"{kvp.Key} = {ArgumentClassNamespace}.{ArgumentArityClassName}.{arity},");
                            break;
                        default:
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                            break;
                    }
                }
            }

            if (AttributeArguments.TryGetValue(AttributeAllowedValuesProperty, out var allowedValuesTypedConstant)
                && !allowedValuesTypedConstant.IsNull)
                sb.AppendLine($"{ArgumentClassNamespace}.ArgumentExtensions.FromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (AttributeArguments.TryGetValue(AttributeAllowExistingProperty, out var allowExistingTypedConstant)
                && allowExistingTypedConstant.Value != null && (bool)allowExistingTypedConstant.Value)
                sb.AppendLine($"{ArgumentClassNamespace}.ArgumentExtensions.ExistingOnly({varName});");

            if (!Required)
                sb.AppendLine($"{varName}.SetDefaultValue({varDefaultValue});");

            //In ArgumentArity.Default, Arity is set to ZeroOrMore for IEnumerable if parent is command,
            //but we want to enforce OneOrMore so that Required is consistent
            if (Required
                && ParseInfo.ItemType != null //if it's a collection type
                && !AttributeArguments.ContainsKey(AttributeArityProperty))
                sb.AppendLine($"{varName}.Arity = {ArgumentClassNamespace}.{ArgumentArityClassName}.OneOrMore;");
        }

        public bool Equals(CliArgumentInfo other)
        {
            return base.Equals(other);
        }

    }
}
