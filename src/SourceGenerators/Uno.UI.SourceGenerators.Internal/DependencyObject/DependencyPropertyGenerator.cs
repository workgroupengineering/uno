﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Uno.Extensions;
using Uno.UI.SourceGenerators.Helpers;
using Uno.UI.SourceGenerators.Internal.Extensions;
using Uno.UI.SourceGenerators.XamlGenerator;

namespace Uno.UI.SourceGenerators.DependencyObject
{
	[Generator]
	public class DependencyPropertyGenerator : IIncrementalGenerator
	{
		private static SymbolDisplayFormat _fullyQualifiedWithoutGlobal = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

		private record AttachedPropertyData
		{
			public string? AttachedBackingFieldOwnerFullyQualifiedName { get; }
			public string? AttachedBackingFieldOwnerNamespace { get; }
			public string? AttachedBackingFieldOwnerName { get; }


			[MemberNotNullWhen(true, nameof(PropertyTypeFullyQualifiedName), nameof(PropertyTargetFullyQualifiedName))]
			public bool HasGetMethodSymbol { get; }

			public bool HasSetMethodSymbol { get; }
			public string? PropertyTypeFullyQualifiedName { get; }
			public string? PropertyTargetFullyQualifiedName { get; }

			public string? GetMethodSymbolNodeContent { get; }
			public string? SetMethodSymbolNodeContent { get; }

			public AttachedPropertyData(ISymbol dpSymbol, AttributeData attribute, string propertyName)
			{
				var symbol = GetAttributeValue(attribute, "AttachedBackingFieldOwner")?.Value.Value as INamedTypeSymbol;
				AttachedBackingFieldOwnerFullyQualifiedName = symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				AttachedBackingFieldOwnerName = symbol?.Name;
				AttachedBackingFieldOwnerNamespace = symbol?.ContainingNamespace.ToString();

				var getMethodSymbol = dpSymbol.ContainingType.GetFirstMethodWithName("Get" + propertyName);
				var setMethodSymbol = dpSymbol.ContainingType.GetFirstMethodWithName("Set" + propertyName);
				GetMethodSymbolNodeContent = SymbolToNodeString(getMethodSymbol);
				SetMethodSymbolNodeContent = SymbolToNodeString(setMethodSymbol);

				HasGetMethodSymbol = getMethodSymbol is not null;
				HasSetMethodSymbol = setMethodSymbol is not null;
				PropertyTypeFullyQualifiedName = getMethodSymbol?.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				PropertyTargetFullyQualifiedName = getMethodSymbol?.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}
		}

		private record PropertyData
		{
			public string? PropertySymbolNodeContent { get; }


			public string? PropertyTypeFullyQualifiedName { get; }

			[MemberNotNullWhen(true, nameof(PropertyTypeFullyQualifiedName))]
			public bool HasProperty { get; }

			public bool PropertyHasGetter { get; }
			public bool PropertyHasSetter { get; }

			public PropertyData(ISymbol dpSymbol, string propertyName)
			{
				var propertySymbol = dpSymbol.ContainingType.GetPropertiesWithName(propertyName).FirstOrDefault();
				PropertySymbolNodeContent = SymbolToNodeString(propertySymbol);

				HasProperty = propertySymbol is not null;
				PropertyTypeFullyQualifiedName = propertySymbol?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

				PropertyHasGetter = propertySymbol?.GetMethod is not null;
				PropertyHasSetter = propertySymbol?.SetMethod is not null;
			}
		}

		private record GenerationCandidateData
		{
			#region Attribute arguments
			public string MetadataOptions { get; }
			public KeyValuePair<string, TypedConstant>? DefaultValue { get; }
			public bool CoerceCallback { get; }
			public bool LocalCache { get; }

			[MemberNotNullWhen(true, nameof(AttachedPropertyData))]
			[MemberNotNullWhen(false, nameof(PropertyData))]
			public bool IsAttached { get; }

			public string ChangedCallbackName { get; }
			#endregion

			public string ContainingNamespace { get; }
			public string ContainingTypeName { get; }
			public string ContainingTypeFullyQualifiedName { get; }
			public string ContainingTypeHintName { get; }
			public bool ContainingTypeIsCandidate { get; }

			public PropertyData? PropertyData { get; }
			public AttachedPropertyData? AttachedPropertyData { get; }

			public bool ContainingTypeHasGetDefaultValueMethod { get; }
			public string PropertyName { get; }
			public bool HasPropertySuffix { get; }

			public int? CoerceCallbackMethodParameterLength { get; }

			public bool IsCallbackWithDPChangedArgs { get; }
			public bool IsCallbackWithDPChangedArgsOnly { get; }
			public bool CallBackWithOldAndNew { get; }
			public bool IsInvalidChangedCallbackName { get; }

			public string? MemberSymbolNodeContent { get; }

			public GenerationCandidateData(ISymbol dpSymbol, AttributeData attribute)
			{
				PropertyName = dpSymbol.Name.TrimEnd("Property");
				HasPropertySuffix = dpSymbol.Name.EndsWith("Property", StringComparison.Ordinal);

				MetadataOptions = GetAttributeValue(attribute, "Options")?.Value.Value?.ToString() ?? "0";
				DefaultValue = GetAttributeValue(attribute, "DefaultValue");
				CoerceCallback = GetBooleanAttributeValue(attribute, "CoerceCallback", false);
				LocalCache = GetBooleanAttributeValue(attribute, "LocalCache", true);
				IsAttached = GetBooleanAttributeValue(attribute, "Attached", false);
				ChangedCallbackName = GetAttributeValue(attribute, "ChangedCallbackName")?.Value.Value?.ToString() ?? $"On{PropertyName}Changed";

				ContainingNamespace = dpSymbol.ContainingNamespace.ToString();
				ContainingTypeName = dpSymbol.ContainingType.Name;

				var isDependencyObject = dpSymbol.ContainingType.AllInterfaces
					.Any(t => t.ToDisplayString(_fullyQualifiedWithoutGlobal) == XamlConstants.Types.DependencyObject);

				ContainingTypeIsCandidate = dpSymbol.ContainingType.TypeKind == TypeKind.Class &&
					(dpSymbol.ContainingType.IsStatic || isDependencyObject);

				ContainingTypeFullyQualifiedName = dpSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				ContainingTypeHintName = HashBuilder.BuildIDFromSymbol(dpSymbol.ContainingType);
				ContainingTypeHasGetDefaultValueMethod = dpSymbol.ContainingType.GetFirstMethodWithName($"Get{PropertyName}DefaultValue") is not null;
				CoerceCallbackMethodParameterLength = dpSymbol.ContainingType.GetFirstMethodWithName("Coerce" + PropertyName)?.Parameters.Length;

				MemberSymbolNodeContent = SymbolToNodeString(dpSymbol);

				var changedCallback = GetBooleanAttributeValue(attribute, "ChangedCallback", false);
				var propertyChangedMethods = dpSymbol.ContainingType.GetMethodsWithName(ChangedCallbackName).ToArray();
				if (changedCallback || propertyChangedMethods.Length > 0)
				{
					if (propertyChangedMethods.Any(m => IsCallbackWithDPChangedArgs(m)))
					{
						IsCallbackWithDPChangedArgs = true;
					}
					else if (propertyChangedMethods.Any(m => IsCallbackWithDPChangedArgsOnly(m)))
					{
						IsCallbackWithDPChangedArgsOnly = true;
					}
					else if (propertyChangedMethods.Any(m => m.Parameters.Length == 2))
					{
						CallBackWithOldAndNew = true;
					}
					else
					{
						IsInvalidChangedCallbackName = true;
					}
				}

				if (IsAttached)
				{
					AttachedPropertyData = new AttachedPropertyData(dpSymbol, attribute, PropertyName);
				}
				else
				{
					PropertyData = new PropertyData(dpSymbol, PropertyName);
				}
			}
		}

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			var attributedSymbolsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
				"Uno.UI.Xaml.GeneratedDependencyPropertyAttribute",
				static (node, _) => node.IsKind(SyntaxKind.PropertyDeclaration),
				static (context, token) =>
				{
					var attribute = context.Attributes[0];
					return new GenerationCandidateData(context.TargetSymbol, attribute);
				});

			var filteredAttributedSymbolsProvider = attributedSymbolsProvider.Where(combined => combined.ContainingTypeIsCandidate);

			var groupedByContainingProvider = filteredAttributedSymbolsProvider
			   .GroupBy(data => data.ContainingTypeFullyQualifiedName, StringComparer.Ordinal);

			context.RegisterSourceOutput(groupedByContainingProvider, GenerateSource);
		}

		private void GenerateSource(SourceProductionContext context, ImmutableArray<GenerationCandidateData> dpCandidatesData)
		{
			if (dpCandidatesData.Length == 0)
			{
				return;
			}

			var builder = new IndentedStringBuilder();
			builder.AppendLineIndented("// <auto-generated>");
			builder.AppendLineIndented("// ******************************************************************");
			builder.AppendLineIndented("// This file has been generated by Uno.UI (DependencyPropertyGenerator)");
			builder.AppendLineIndented("// ******************************************************************");
			builder.AppendLineIndented("// </auto-generated>");
			builder.AppendLine();
			builder.AppendLineIndented("#pragma warning disable 1591 // Ignore missing XML comment warnings");
			builder.AppendLineIndented("using System;");
			builder.AppendLineIndented("using System.Linq;");
			builder.AppendLineIndented("using System.Collections.Generic;");
			builder.AppendLineIndented("using System.Collections;");
			builder.AppendLineIndented("using System.Diagnostics.CodeAnalysis;");
			builder.AppendLineIndented("using Uno.Disposables;");
			builder.AppendLineIndented("using System.Runtime.CompilerServices;");
			builder.AppendLineIndented("using Uno.UI;");
			builder.AppendLineIndented("using Uno.UI.DataBinding;");
			builder.AppendLineIndented("using Microsoft.UI.Xaml;");
			builder.AppendLineIndented("using Microsoft.UI.Xaml.Controls;");
			builder.AppendLineIndented("using Microsoft.UI.Xaml.Data;");
			builder.AppendLineIndented("using Uno.Diagnostics.Eventing;");

			var attachedPropertiesBackingFieldStatements = new Dictionary<(string ContainingNamespace, string Name), List<string>>();

			using (builder.BlockInvariant($"namespace {dpCandidatesData[0].ContainingNamespace}"))
			{
				using (builder.BlockInvariant($"partial class {dpCandidatesData[0].ContainingTypeName}"))
				{
					foreach (var dpCandidateData in dpCandidatesData)
					{
						if (!dpCandidateData.HasPropertySuffix)
						{
							builder.AppendLineIndented("#error Property name should end with 'Property' suffix'");
						}
						else if (dpCandidateData.IsAttached)
						{
							GenerateAttachedProperty(builder, dpCandidateData, dpCandidateData.AttachedPropertyData, attachedPropertiesBackingFieldStatements);
						}
						else
						{
							GenerateProperty(builder, dpCandidateData, dpCandidateData.PropertyData);
						}
					}
				}
			}

			foreach (var backingFieldType in attachedPropertiesBackingFieldStatements)
			{
				using (builder.BlockInvariant($"namespace {backingFieldType.Key.ContainingNamespace}"))
				{
					using (builder.BlockInvariant($"partial class {backingFieldType.Key.Name}"))
					{
						foreach (var statement in backingFieldType.Value)
						{
							builder.AppendLineIndented(statement);
						}
					}
				}
			}

			context.AddSource(dpCandidatesData[0].ContainingTypeHintName, builder.ToString());
		}

		private static void GenerateAttachedProperty(IndentedStringBuilder builder, GenerationCandidateData data, AttachedPropertyData attachedPropertyData, Dictionary<(string ContainingNamespace, string Name), List<string>> backingFieldStatements)
		{
			var propertyName = data.PropertyName;

			if (!attachedPropertyData.HasGetMethodSymbol)
			{
				builder.AppendLineIndented($"#error unable to find getter method for {propertyName} on {data.ContainingTypeFullyQualifiedName}");
				return;
			}

			if (!attachedPropertyData.HasSetMethodSymbol)
			{
				builder.AppendLineIndented($"#error unable to find setter method for {propertyName} on {data.ContainingTypeFullyQualifiedName}");
				return;
			}

			var coerceCallback = data.CoerceCallback;
			var localCache = data.LocalCache;
			var changedCallbackName = data.ChangedCallbackName;

			var propertyTypeName = attachedPropertyData.PropertyTypeFullyQualifiedName;
			var propertyOwnerTypeName = data.ContainingTypeFullyQualifiedName;
			var propertyTargetName = attachedPropertyData.PropertyTargetFullyQualifiedName;

			var backingFieldOwnerTypeName = attachedPropertyData.AttachedBackingFieldOwnerFullyQualifiedName;
			var backingFieldName = $"__{data.ContainingTypeName}_{propertyName}PropertyBackingField";

			ValidateInvocation(builder, data.MemberSymbolNodeContent, data.PropertyName, $"Create{propertyName}Property");
			ValidateInvocation(builder, attachedPropertyData.GetMethodSymbolNodeContent, "Get" + propertyName, $"Get{propertyName}Value");
			ValidateInvocation(builder, attachedPropertyData.SetMethodSymbolNodeContent, "Set" + propertyName, $"Set{propertyName}Value");

			builder.AppendLineIndented($"#region {propertyName} Dependency Property");

			using (builder.BlockInvariant($"private static {propertyTypeName} Get{propertyName}Value({propertyTargetName} instance)"))
			{
				if (localCache)
				{
					if (backingFieldOwnerTypeName is null)
					{
						builder.AppendLineIndented($"#error local cache methods must have AttachedBackingFieldOwner set");
						return;
					}

					var key = (attachedPropertyData.AttachedBackingFieldOwnerNamespace!, attachedPropertyData.AttachedBackingFieldOwnerName!);
					if (!backingFieldStatements.TryGetValue(key, out var statementList))
					{
						statementList = backingFieldStatements[key] = new List<string>();
					}

					statementList.Add($"internal bool {backingFieldName}Set;");
					statementList.Add($"internal {propertyTypeName} {backingFieldName};");

					using (builder.BlockInvariant($"if(instance is {backingFieldOwnerTypeName} backingFieldOwnerInstance)"))
					{
						using (builder.BlockInvariant($"if (!backingFieldOwnerInstance.{backingFieldName}Set)"))
						{
							builder.AppendLineIndented($"backingFieldOwnerInstance.{backingFieldName} = ({propertyTypeName})instance.GetValue({propertyOwnerTypeName}.{propertyName}Property);");
							builder.AppendLineIndented($"backingFieldOwnerInstance.{backingFieldName}Set = true;");
						}

						builder.AppendLineIndented($"return backingFieldOwnerInstance.{backingFieldName};");
					}
					builder.AppendLineIndented($"else");
					using (builder.BlockInvariant(""))
					{
						builder.AppendLineIndented($"return ({propertyTypeName})instance.GetValue({propertyOwnerTypeName}.{propertyName}Property);");
					}
				}
				else
				{
					builder.AppendLineIndented($"return instance.GetValue({propertyOwnerTypeName}.{propertyName}Property);");
				}
			}

			builder.AppendLineIndented($"private static void Set{propertyName}Value({propertyTargetName} instance, {propertyTypeName} value) => instance.SetValue({propertyOwnerTypeName}.{propertyName}Property, value);");

			GeneratePropertyStorage(builder, propertyName);

			builder.AppendLineIndented($"DependencyProperty.RegisterAttached(");

			BuildPropertyParameters(builder, data, propertyTypeName);

			if (localCache)
			{
				using (builder.BlockInvariant($"\t\t, backingFieldUpdateCallback: (instance, newValue) => "))
				{
					using (builder.BlockInvariant($"if(instance is {backingFieldOwnerTypeName} backingFieldOwnerInstance)"))
					{
						builder.AppendLineIndented($"backingFieldOwnerInstance.{backingFieldName} = ({propertyTypeName})instance.GetValue({propertyOwnerTypeName}.{propertyName}Property);");
						builder.AppendLineIndented($"backingFieldOwnerInstance.{backingFieldName}Set = true;");
					}
				}
			}

			if (coerceCallback || data.CoerceCallbackMethodParameterLength is not null)
			{
				if (data.CoerceCallbackMethodParameterLength is 2)
				{
					builder.AppendLineIndented($"\t\t, coerceValueCallback: (instance, baseValue, precedence) => Coerce{propertyName}(instance, ({propertyTypeName})baseValue, precedence)");
				}
				else
				{
					builder.AppendLineIndented($"\t\t, coerceValueCallback: (instance, baseValue, precedence) => Coerce{propertyName}(instance, ({propertyTypeName})baseValue)");
				}
			}

			if (data.IsCallbackWithDPChangedArgs)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => {changedCallbackName}(instance, args)");
			}
			else if (data.IsCallbackWithDPChangedArgsOnly)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => {changedCallbackName}(args)");
			}
			else if (data.CallBackWithOldAndNew)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => {changedCallbackName}(({propertyTypeName})args.OldValue, ({propertyTypeName})args.NewValue)");
			}
			else if (data.IsInvalidChangedCallbackName)
			{
				builder.AppendLineIndented($"#error Valid {changedCallbackName} not found.  Must be {changedCallbackName}(DependencyPropertyChangedEventArgs), {changedCallbackName}(Instance, DependencyPropertyChangedEventArgs) or {changedCallbackName}(oldValue, newValue)");
			}

			builder.AppendLineIndented($"));");

			builder.AppendLineIndented($"#endregion");
		}

		private static void GenerateProperty(IndentedStringBuilder builder, GenerationCandidateData data, PropertyData propertyData)
		{
			var propertyName = data.PropertyName;
			if (!propertyData.HasProperty)
			{
				builder.AppendLineIndented($"#error unable to find property {propertyName} on {data.ContainingTypeFullyQualifiedName}");
				return;
			}

			var propertyTypeName = propertyData.PropertyTypeFullyQualifiedName;
			var containingTypeName = data.ContainingTypeFullyQualifiedName;
			var changedCallbackName = data.ChangedCallbackName;
			var coerceCallback = data.CoerceCallback;
			var localCache = data.LocalCache;

			ValidateInvocation(builder, propertyData.PropertySymbolNodeContent, data.PropertyName, $"Get{propertyName}Value", $"Set{propertyName}Value");
			ValidateInvocation(builder, data.MemberSymbolNodeContent, data.PropertyName + "Property", $"Create{propertyName}Property");

			builder.AppendLineIndented($"#region {propertyName} Dependency Property");

			if (propertyData.PropertyHasGetter)
			{
				using (builder.BlockInvariant($"private {propertyTypeName} Get{propertyName}Value()"))
				{
					if (localCache)
					{
						using (builder.BlockInvariant($"if (!_{propertyName}PropertyBackingFieldSet)"))
						{
							builder.AppendLineIndented($"_{propertyName}PropertyBackingField = ({propertyTypeName})GetValue({propertyName}Property);");
							builder.AppendLineIndented($"_{propertyName}PropertyBackingFieldSet = true;");
						}

						builder.AppendLineIndented($"return _{propertyName}PropertyBackingField;");
					}
					else
					{
						builder.AppendLineIndented($"return ({propertyTypeName})GetValue({propertyName}Property);");
					}
				}
			}

			if (propertyData.PropertyHasSetter)
			{
				builder.AppendLineIndented($"private void Set{propertyName}Value({propertyTypeName} value) => SetValue({propertyName}Property, value);");
			}

			if (localCache)
			{
				builder.AppendLineIndented($"private bool _{propertyName}PropertyBackingFieldSet = false;");
				builder.AppendLineIndented($"private {propertyTypeName} _{propertyName}PropertyBackingField;");
			}

			GeneratePropertyStorage(builder, propertyName);

			builder.AppendLineIndented($"DependencyProperty.Register(");

			BuildPropertyParameters(builder, data, propertyTypeName);

			if (localCache)
			{
				// Use a explicit delegate to avoid C# delegate caching (the delegate is kept in the DP, no need to cache it in the class)
				builder.AppendLineIndented($"\t\t, backingFieldUpdateCallback: On{propertyName}BackingFieldUpdate");
			}

			if (coerceCallback || data.CoerceCallbackMethodParameterLength is not null)
			{
				if (data.CoerceCallbackMethodParameterLength is 2)
				{
					builder.AppendLineIndented($"\t\t, coerceValueCallback: (instance, baseValue, precedence) => (({containingTypeName})instance).Coerce{propertyName}(baseValue, precedence)");
				}
				else
				{
					builder.AppendLineIndented($"\t\t, coerceValueCallback: (instance, baseValue, precedence) => (({containingTypeName})instance).Coerce{propertyName}(baseValue)");
				}
			}


			if (data.IsCallbackWithDPChangedArgs)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => (({containingTypeName})instance).{changedCallbackName}(instance, args)");
			}
			else if (data.IsCallbackWithDPChangedArgsOnly)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => (({containingTypeName})instance).{changedCallbackName}(args)");
			}
			else if (data.CallBackWithOldAndNew)
			{
				builder.AppendLineIndented($"\t\t, propertyChangedCallback: (instance, args) => (({containingTypeName})instance).{changedCallbackName}(({propertyTypeName})args.OldValue, ({propertyTypeName})args.NewValue)");
			}
			else if (data.IsInvalidChangedCallbackName)
			{
				builder.AppendLineIndented($"#error Valid {changedCallbackName} not found.  Must be {changedCallbackName}(DependencyPropertyChangedEventArgs), {changedCallbackName}(Instance, DependencyPropertyChangedEventArgs) or {changedCallbackName}(oldValue, newValue)");
			}

			builder.AppendLineIndented($"));");

			if (localCache)
			{
				using (builder.BlockInvariant($"private static void On{propertyName}BackingFieldUpdate(object instance, object newValue)"))
				{
					builder.AppendLineIndented($"var typedInstance = instance as {containingTypeName};");
					builder.AppendLineIndented($"typedInstance._{propertyName}PropertyBackingField = ({propertyTypeName})newValue;");
					builder.AppendLineIndented($"typedInstance._{propertyName}PropertyBackingFieldSet = true;");
				}
			}

			builder.AppendLineIndented($"#endregion");
		}

		private static string? SymbolToNodeString(ISymbol? symbol)
		{
			if (symbol?.Locations.FirstOrDefault() is Location location)
			{
				if (location.SourceTree != null)
				{
					var node = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
					return node.ToString();
				}
			}

			return null;
		}

		private static void ValidateInvocation(IndentedStringBuilder builder, string? syntaxNodeContent, string propertySymbol, params string[] invocations)
		{
			if (syntaxNodeContent is not null)
			{
				if (!invocations.All(l => syntaxNodeContent.Contains(l, StringComparison.Ordinal)))
				{
					var invocationsMessage = string.Join(", ", invocations);
					builder.AppendLineIndented($"#error unable to find some of the following statements {invocationsMessage} in {propertySymbol}");
				}
			}
		}

		private static void GeneratePropertyStorage(IndentedStringBuilder builder, string propertyName)
		{
			builder.AppendLineIndented($"/// <summary>");
			builder.AppendLineIndented($"/// Generated method used to create the <see cref=\"{propertyName}Property\" /> member value");
			builder.AppendLineIndented($"/// </summary>");
			builder.AppendLineIndented($"private static global::Microsoft.UI.Xaml.DependencyProperty Create{propertyName}Property() => ");
		}

		private static void BuildPropertyParameters(
			IndentedStringBuilder builder,
			GenerationCandidateData data,
			string propertyTypeName)
		{
			var propertyName = data.PropertyName;
			var metadataOptions = data.MetadataOptions;
			var defaultValue = data.DefaultValue;
			builder.AppendLineIndented($"\tname: \"{propertyName}\",");
			builder.AppendLineIndented($"\tpropertyType: typeof({propertyTypeName}),");
			builder.AppendLineIndented($"\townerType: typeof({data.ContainingTypeFullyQualifiedName}),");
			builder.AppendLineIndented($"\ttypeMetadata: new global::Microsoft.UI.Xaml.FrameworkPropertyMetadata(");

			if (defaultValue.HasValue && !string.IsNullOrEmpty(defaultValue.Value.Key))
			{
				var defaultValueMethodName = $"Get{propertyName}DefaultValue()";
				if (data.ContainingTypeHasGetDefaultValueMethod)
				{
					builder.AppendLineIndented($"#error The generated property {propertyName} cannot contains both a DefaultValue and the {defaultValueMethodName} method.");
				}

				var defaultValueString = defaultValue.Value.Value.Value switch
				{
					string s => $"\"{s}\"",
					double d when double.IsPositiveInfinity(d) => "double.PositiveInfinity",
					double d when double.IsNegativeInfinity(d) => "double.NegativInfinity",
					double d when double.IsNaN(d) => "double.NaN",
					double d => d.ToString(CultureInfo.InvariantCulture),
					float d when float.IsPositiveInfinity(d) => "float.PositiveInfinity",
					float d when float.IsNegativeInfinity(d) => "float.NegativInfinity",
					float d when float.IsNaN(d) => "float.NaN",
					float d => d.ToString(CultureInfo.InvariantCulture) + "f",
					bool d => d.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
					var o => o?.ToString() ?? "null",
				};

				builder.AppendLineIndented($"\t\tdefaultValue: ({propertyTypeName}){defaultValueString} /* {defaultValueMethodName}, {data.ContainingTypeFullyQualifiedName} */");
			}
			else
			{
				builder.AppendLineIndented($"\t\tdefaultValue: Get{propertyName}DefaultValue()");
			}

			if (metadataOptions != "0")
			{
				builder.AppendLineIndented($"\t\t, options: (global::Microsoft.UI.Xaml.FrameworkPropertyMetadataOptions){metadataOptions}");
			}
		}

		private static bool IsCallbackWithDPChangedArgsOnly(IMethodSymbol m)
			=> m.Parameters.Length > 0 && m.Parameters[0].Type.ToDisplayString(_fullyQualifiedWithoutGlobal) == "Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs";

		private static bool IsCallbackWithDPChangedArgs(IMethodSymbol m)
			=> m.Parameters.Length == 2 && m.Parameters[1].Type.ToDisplayString(_fullyQualifiedWithoutGlobal) == "Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs";

		private static KeyValuePair<string, TypedConstant>? GetAttributeValue(AttributeData attribute, string parameterName)
			=> attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == parameterName);

		private static bool GetBooleanAttributeValue(AttributeData attribute, string parameterName, bool defaultValue)
			=> attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == parameterName).Value.Value is bool value ? value : defaultValue;
	}
}
