﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;
using Uno.UI.SourceGenerators.Tests.Verifiers;

namespace Uno.UI.SourceGenerators.Tests.XamlCodeGeneratorTests;

using Verify = XamlSourceGeneratorVerifier;

[TestClass]
public class Given_AttachedPropertyWithNoFuzzyMatching
{
	[TestMethod]
	public async Task TestVisibleBoundsPadding()
	{
		var xamlFile = new XamlFile("MainPage.xaml", """
			<Page x:Class="TestRepro.MainPage"
					xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
					xmlns:toolkit="using:Uno.UI.Toolkit"
					mc:Ignorable="android ios">

				<Grid toolkit:VisibleBoundsPadding.PaddingMask="Top">
					<TextBlock Text="Hello, world!"
							   Margin="20"
							   FontSize="30" />
				</Grid>

			</Page>
			""");

		var test = new Verify.Test(xamlFile)
		{
			TestState =
			{
				Sources =
				{
					"""
					using Microsoft.UI.Xaml;
					using Microsoft.UI.Xaml.Controls;

					namespace TestRepro
					{
						public sealed partial class MainPage : Page
						{
							public MainPage()
							{
								this.InitializeComponent();
							}
						}
					}
					"""
				}
			},
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60Android.AddPackages(ImmutableArray.Create(new PackageIdentity("Uno.UI", "4.9.26"))),
			DisableBuildReferences = true,
		}.AddGeneratedSources();

		await test.RunAsync();
	}
}
