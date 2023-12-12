﻿using Microsoft.CodeAnalysis.Testing;
using Uno.UI.SourceGenerators.Tests.Verifiers;

namespace Uno.UI.SourceGenerators.Tests.Windows_UI_Xaml_Data.BindingTests;

using Verify = XamlSourceGeneratorVerifier;

[TestClass]
public class Given_Binding
{
	[TestMethod]
	public async Task When_Xaml_Object_With_Common_Properties()
	{
		var test = new TestSetup(xamlFileName: "Binding_Xaml_Object_With_Common_Properties.xaml", subFolder: Path.Combine("Uno.UI.Tests", "Windows_UI_Xaml_Data", "BindingTests", "Controls"));
		await Verify.AssertXamlGenerator(test);
	}

	[TestMethod]
	public async Task When_Xaml_Object_With_Xaml_Object_Properties()
	{
		var test = new TestSetup(xamlFileName: "Binding_Xaml_Object_With_Xaml_Object_Properties.xaml", subFolder: Path.Combine("Uno.UI.Tests", "Windows_UI_Xaml_Data", "BindingTests", "Controls"));
		await Verify.AssertXamlGenerator(test);
	}

	[TestMethod]
	public async Task When_Binding_ElementName_In_Template()
	{
		var test = new TestSetup(xamlFileName: "Binding_ElementName_In_Template.xaml", subFolder: Path.Combine("Uno.UI.Tests", "Windows_UI_Xaml_Data", "BindingTests", "Controls"))
		{
			PreprocessorSymbols =
			{
				"UNO_REFERENCE_API",
			},
		};
		await Verify.AssertXamlGenerator(test);
	}

	[TestMethod]
	public async Task TestBaseTypeNotSpecifiedInCodeBehind()
	{
		var xamlFiles = new[]
		{
			new XamlFile("UserControl1.xaml", """
	<UserControl
		x:Class="TestRepro.UserControl1"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:TestRepro"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d"
		d:DesignHeight="300"
		d:DesignWidth="400">

		<Grid></Grid>
	</UserControl>
	"""),
			new XamlFile("MainPage.xaml", """
	<Page
		x:Class="TestRepro.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:TestRepro"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d"
		Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

		<Grid>
			<TextBlock Text="Hello, world!" Margin="20" FontSize="30" />
			<local:UserControl1 DataContext="{Binding PreviewDropViewModel}"/>
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
					namespace TestRepro
					{
						public sealed partial class UserControl1
						{
							public UserControl1()
							{
								this.InitializeComponent();
							}
						}
					}
					""",
					"""
					using Microsoft.UI.Xaml.Controls;

					namespace TestRepro
					{
						public sealed partial class MainPage : Page
						{
							public string PreviewDropViewModel { get; set; }

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
		test.ExpectedDiagnostics.Add(
			// /0/Test0.cs(3,30): warning UXAML0002: TestRepro.UserControl1 does not explicitly define the Microsoft.UI.Xaml.Controls.UserControl base type in code behind.
			DiagnosticResult.CompilerWarning("UXAML0002").WithSpan(3, 30, 3, 42).WithArguments("TestRepro.UserControl1 does not explicitly define the Microsoft.UI.Xaml.Controls.UserControl base type in code behind.")
		);
		await test.RunAsync();
	}

	[TestMethod]
	public async Task TestAccessingArray()
	{
		var xamlFiles = new[]
		{
			new XamlFile("MainPage.xaml", """
	<Page
		x:Class="TestRepro.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:TestRepro"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d"
		Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

		<Grid>
			<TextBlock Text="{x:Bind MyArray.Length}" />
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
							public string[] MyArray { get; set; }

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

		await test.RunAsync();
	}

	[TestMethod]
	public async Task TestInheritedMethod()
	{
		var xamlFile = new XamlFile("MainPage.xaml", """
	<Page
		x:Class="TestRepro.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:TestRepro"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d"
		Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

		<Grid>
			<Button Click="{x:Bind P1.Button_Click, Mode=OneWay}" />
			<Button Click="{x:Bind P2.Button_Click, Mode=OneWay}" />
			<Button Click="{x:Bind P3.Button_Click, Mode=OneWay}" />
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
						public class C1
						{
							public void Button_Click(object sender, RoutedEventArgs e)
							{

							}
						}

						public class C2 : C1 { }

						public interface I
						{
							void Button_Click(object sender, RoutedEventArgs e);
						}

						public class ImplicitImpl : I
						{
							public void Button_Click(object sender, RoutedEventArgs e)
							{
					
							}	
						}

						public class ExplicitImpl : I
						{
							void I.Button_Click(object sender, RoutedEventArgs e)
							{
					
							}	
						}

						public sealed partial class MainPage : Page
						{
							public C2 P1 { get; } = new C2();
							public ImplicitImpl P2 { get; } = new ImplicitImpl();
							public I P3 { get; } = new ExplicitImpl();

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

		await test.RunAsync();
	}

	[TestMethod]
	public async Task TestTwoWay()
	{
		var xamlFiles = new[]
		{
			new XamlFile("MainPage.xaml", """
	<Page
		x:Class="TestRepro.MainPage"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:local="using:TestRepro"
		xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
		xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
		mc:Ignorable="d"
		Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

		<Grid>
			<TextBlock Text="{x:Bind ViewModel.P, Mode=TwoWay}" />
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
						public class VM
						{
							public string P { get; set; }
						}

						public sealed partial class MainPage : Page
						{
							public VM ViewModel { get; set; }

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

		await test.RunAsync();
	}

	[TestMethod]
	public async Task TestDefaultBindingModeInDataTemplateInsideResourceDictionary()
	{
		var xamlFile = new XamlFile("MyResourceDictionary.xaml", """
			<ResourceDictionary
				x:Class="TestRepro.MyResourceDictionary"
				xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
				xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
				xmlns:local="using:TestRepro"
				xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
				xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
				x:DefaultBindMode="OneWay"
				mc:Ignorable="d">

					<DataTemplate x:Key="myTemplate" x:DataType="local:MyModel">
						<TextBlock x:Name="tb" Text="{x:Bind MyString}" />
					</DataTemplate>
			</ResourceDictionary>
			""");

		var test = new Verify.Test(xamlFile)
		{
			TestState =
			{
				Sources =
				{
					"""
					using Microsoft.UI.Xaml;

					namespace TestRepro
					{
						public sealed partial class MyResourceDictionary : ResourceDictionary
						{
							public MyResourceDictionary()
							{
								this.InitializeComponent();
							}
						}

						public partial class MyModel
						{
							public string MyString
							{
								get => throw new System.NotImplementedException();
								set => throw new System.NotImplementedException();
							}

							public static readonly DependencyProperty MyStringProperty =
								DependencyProperty.Register(nameof(MyString), typeof(string), typeof(MyModel), new FrameworkPropertyMetadata(null));
						}
					}

					"""
				}
			}
		}.AddGeneratedSources();

		await test.RunAsync();
	}
}
