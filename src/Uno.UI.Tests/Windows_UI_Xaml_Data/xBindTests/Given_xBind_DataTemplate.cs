﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UI.Tests.Windows_UI_Xaml_Data.xBindTests.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Uno.UI.Tests.Windows_UI_Xaml_Data.xBindTests
{
	[TestClass]
	public class Given_xBind_DataTemplate
	{
		[TestMethod]
		public void When_Initial_Value()
		{
			var SUT = new DataTemplate_Control();
			SUT.ForceLoaded();

			var _MyProperty = SUT.FindName("_MyProperty") as TextBlock;
			Assert.AreEqual("Initial", _MyProperty.Text);

			var _MyProperty_Function = SUT.FindName("_MyProperty_Function") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);

			var _MyProperty_Formatted = SUT.FindName("_MyProperty_Formatted") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);
		}

		[TestMethod]
		public void When_Updated_Source()
		{
			var SUT = new DataTemplate_Control();
			SUT.ForceLoaded();

			var _MyProperty = SUT.FindName("_MyProperty") as TextBlock;
			Assert.AreEqual("Initial", _MyProperty.Text);

			var _MyProperty_Function = SUT.FindName("_MyProperty_Function") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);

			var _MyProperty_Formatted = SUT.FindName("_MyProperty_Formatted") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);

			SUT.root.Content = new MyDataTemplateClass() { MyProperty = "Value 2" };

			Assert.AreEqual("Value 2", _MyProperty.Text);
			Assert.AreEqual("VALUE 2", _MyProperty_Function.Text);
			Assert.AreEqual("Formatted Value 2", _MyProperty_Formatted.Text);
		}

		[TestMethod]
		public void When_Updated_Property()
		{
			var SUT = new DataTemplate_Control();
			var data = new MyDataTemplateClass();
			SUT.root.Content = data;

			SUT.ForceLoaded();

			var _MyProperty = SUT.FindName("_MyProperty") as TextBlock;
			Assert.AreEqual("Initial", _MyProperty.Text);

			var _MyProperty_Function = SUT.FindName("_MyProperty_Function") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);

			var _MyProperty_Formatted = SUT.FindName("_MyProperty_Formatted") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);

			var _MyProperty_Function_OneWay = SUT.FindName("_MyProperty_Function_OneWay") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function_OneWay.Text);

			var _MyProperty_Formatted_OneWay = SUT.FindName("_MyProperty_Formatted_OneWay") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted_OneWay.Text);

			Assert.AreEqual(9, data.MyPropertyGetCounter);

			data.MyProperty = "Other value";

			Assert.AreEqual("Initial", _MyProperty.Text);
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);
			Assert.AreEqual("OTHER VALUE", _MyProperty_Function_OneWay.Text);
			Assert.AreEqual("Formatted Other value", _MyProperty_Formatted_OneWay.Text);

			Assert.AreEqual(13, data.MyPropertyGetCounter);
		}

		[TestMethod]
		public void When_Updated_With_Null()
		{
			var SUT = new DataTemplate_Control();
			var data = new MyDataTemplateClass();
			SUT.root.Content = data;

			SUT.ForceLoaded();

			var _MyProperty = SUT.FindName("_MyProperty") as TextBlock;
			Assert.AreEqual("Initial", _MyProperty.Text);

			var _MyProperty_Function = SUT.FindName("_MyProperty_Function") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);

			var _MyProperty_Formatted = SUT.FindName("_MyProperty_Formatted") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);

			var _MyProperty_Function_OneWay = SUT.FindName("_MyProperty_Function_OneWay") as TextBlock;
			Assert.AreEqual("INITIAL", _MyProperty_Function_OneWay.Text);

			var _MyProperty_Formatted_OneWay = SUT.FindName("_MyProperty_Formatted_OneWay") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted_OneWay.Text);

			Assert.AreEqual(9, data.MyPropertyGetCounter);

			data.MyProperty = null;

			Assert.AreEqual("Initial", _MyProperty.Text);
			Assert.AreEqual("INITIAL", _MyProperty_Function.Text);
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted.Text);
			Assert.IsNull(null, _MyProperty_Function_OneWay.Text);
			Assert.AreEqual("Formatted ", _MyProperty_Formatted_OneWay.Text);

			Assert.AreEqual(13, data.MyPropertyGetCounter);
		}

		[TestMethod]
		public void When_Loaded_Then_Reset()
		{
			var SUT = new DataTemplate_Control();
			var data = new MyDataTemplateClass();
			SUT.root.Content = data;

			SUT.ForceLoaded();

			Assert.IsTrue(data.HasPropertyChangedListeners);

			var _MyProperty_Formatted_OneWay = SUT.FindName("_MyProperty_Formatted_OneWay") as TextBlock;
			Assert.AreEqual("Formatted Initial", _MyProperty_Formatted_OneWay.Text);

			data.MyProperty = "Other value";

			Assert.AreEqual("Formatted Other value", _MyProperty_Formatted_OneWay.Text);

			SUT.root.Content = null;

			Assert.IsFalse(data.HasPropertyChangedListeners);
		}

		[TestMethod]
		public void When_DataTemplate_StaticProperty()
		{
			var SUT = new DataTemplate_StaticProperty_Control();
			var data = new DataTemplate_StaticProperty_Control_Data();
			SUT.root.Content = data;

			SUT.ForceLoaded();

			var _MyProperty = SUT.FindName("_MyProperty") as TextBlock;
			Assert.AreEqual("Hello", _MyProperty.Text);
		}
	}
}
