﻿using Microsoft.CodeAnalysis.Testing;
using Uno.UI.SourceGenerators.Tests.Verifiers;

namespace Uno.UI.SourceGenerators.Tests.Windows_UI_Xaml_Controls.ParserTests;

using Verify = XamlSourceGeneratorVerifier;

[TestClass]
public class Given_Parser
{
	[TestMethod]
	public async Task When_Invalid_Element_Property()
	{
		var xamlFiles = new[]
		{
			new XamlFile(
				"MainPage.xaml",
				"""
				<Page x:Class="TestRepro.MainPage"
					  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">

					<Grid>

						<NavigationView IsBackButtonVisible="Collapsed"
										IsPaneToggleButtonVisible="False"
										IsSettingsVisible="False"
										PaneDisplayMode="Left">

							<NavigationView.PaneTitle FontSize="16"
													  FontWeight="Bold"
													  Text="PaneTitle" />

						</NavigationView>

					</Grid>
				</Page>
				"""),
		};

		var test = new Verify.Test(xamlFiles)
		{
			TestState =
			{
				Sources =
				{
					"""
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
			}
		}.AddGeneratedSources();

		test.ExpectedDiagnostics.AddRange(
			new[] {
				// /0/MainPage.xaml(13,5): error UXAML0001: Member 'PaneTitle' cannot have properties at line 13, position 5
				DiagnosticResult.CompilerError("UXAML0001").WithSpan("C:/Project/0/MainPage.xaml", 13, 5, 13, 5).WithArguments("Member 'PaneTitle' cannot have properties at line 13, position 5"),
				// /0/Test0.cs(9,9): error CS1061: 'MainPage' does not contain a definition for 'InitializeComponent' and no accessible extension method 'InitializeComponent' accepting a first argument of type 'MainPage' could be found (are you missing a using directive or an assembly reference?)
				DiagnosticResult.CompilerError("CS1061").WithSpan(9, 9, 9, 28).WithArguments("TestRepro.MainPage", "InitializeComponent")
			}
		);
		await test.RunAsync();
	}

	[TestMethod]
	public async Task When_Namespace_Is_On_Nested_Element()
	{
		var xamlFiles = new[]
		{
			new XamlFile(
				"MainPage.xaml",
				"""
				<Page x:Class="TestRepro.MainPage"
					  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					  xmlns:local="using:TestRepro"
					  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">

					<Grid>

						<local:MyStackPanel xmlns:SUT="NamespaceUnderTest" Source="SUT:C" />

					</Grid>
				</Page>
				"""),
		};

		var test = new Verify.Test(xamlFiles)
		{
			TestState =
			{
				Sources =
				{
					"""
					using System;
					using Microsoft.UI.Xaml.Controls;

					namespace TestRepro
					{

						public sealed partial class MyStackPanel : StackPanel
						{
							public Type Source { get; set; }
						}

						public sealed partial class MainPage : Page
						{
							public MainPage()
							{
								this.InitializeComponent();
							}
						}
					}

					namespace NamespaceUnderTest
					{
						public class C { }
					}
					"""
				}
			}
		}.AddGeneratedSources();

		await test.RunAsync();
	}
}
