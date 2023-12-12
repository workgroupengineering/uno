﻿#nullable enable
#if !WINDOWS_UWP
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Private.Infrastructure;
using Uno.UI.Extensions;
using Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_xLoad
	{
		[TestMethod]
		[RunsOnUIThread]
		public void When_xLoad_Literal()
		{
			var sut = new xLoad_Literal();

			TestServices.WindowHelper.WindowContent = sut;
			var loadBorderFalse = sut.LoadBorderFalse;
			var loadBorderTrue = sut.LoadBorderTrue;

			Assert.IsNull(loadBorderFalse);
			Assert.IsNotNull(loadBorderTrue);
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_xLoad_Order()
		{
			var sut = new When_xLoad_Order();

			TestServices.WindowHelper.WindowContent = sut;

			Assert.IsInstanceOfType(sut.root.Children[0], typeof(ElementStub));
			Assert.IsInstanceOfType(sut.root.Children[1], typeof(ElementStub));
			Assert.IsInstanceOfType(sut.root.Children[2], typeof(ElementStub));

			sut.IsLoaded2 = true;
			sut.Refresh();

			await TestServices.WindowHelper.WaitForIdle();

			Assert.IsInstanceOfType(sut.root.Children[0], typeof(ElementStub));
			Assert.IsInstanceOfType(sut.root.Children[1], typeof(Border));
			Assert.IsInstanceOfType(sut.root.Children[2], typeof(ElementStub));

			sut.IsLoaded3 = true;
			sut.Refresh();

			await TestServices.WindowHelper.WaitForIdle();

			Assert.IsInstanceOfType(sut.root.Children[0], typeof(ElementStub));
			Assert.IsInstanceOfType(sut.root.Children[1], typeof(Border));
			Assert.IsInstanceOfType(sut.root.Children[2], typeof(Border));

			sut.IsLoaded1 = true;
			sut.Refresh();

			await TestServices.WindowHelper.WaitForIdle();

			Assert.IsInstanceOfType(sut.root.Children[0], typeof(Border));
			Assert.IsInstanceOfType(sut.root.Children[1], typeof(Border));
			Assert.IsInstanceOfType(sut.root.Children[2], typeof(Border));
		}

		[TestMethod]
		[RunsOnUIThread]
		public async Task When_xLoad_xBind()
		{
			var sut = new xLoad_xBind();

			TestServices.WindowHelper.WindowContent = sut;
			await TestServices.WindowHelper.WaitForLoaded(sut);

			var loadBorder = sut.LoadBorder;
			Assert.IsNull(sut.LoadBorder);

			sut.IsLoad = true;

			Assert.IsNotNull(sut.LoadBorder);
			var parent = (Border)sut.LoadBorder.Parent;

			sut.IsLoad = false;

			Assert.IsFalse(((ElementStub)parent.Child).Load);

			sut.IsLoad = true;

			Assert.IsNotNull(sut.LoadBorder);
			parent = (Border)sut.LoadBorder.Parent;

			sut.IsLoad = false;

			Assert.IsFalse(((ElementStub)parent.Child).Load);
		}

		[TestMethod]
		[RunsOnUIThread]
		public void When_xLoad_Visibility_While_Materializing()
		{
			var SUT = new When_xLoad_Visibility_While_Materializing();

			Assert.AreEqual(0, When_xLoad_Visibility_While_Materializing_Content.Instances);

			TestServices.WindowHelper.WindowContent = SUT;

			Assert.AreEqual(0, When_xLoad_Visibility_While_Materializing_Content.Instances);

			SUT.Model.IsVisible = true;

			Assert.AreEqual(1, When_xLoad_Visibility_While_Materializing_Content.Instances);
		}

		[TestMethod]
		[RunsOnUIThread]
		public void When_xLoad_xBind_xLoad_Initial()
		{
			var grid = new Grid();
			TestServices.WindowHelper.WindowContent = grid;

			var SUT = new When_xLoad_xBind_xLoad_Initial();
			grid.Children.Add(SUT);

			Assert.IsNotNull(SUT.tb01);
			Assert.AreEqual(1, SUT.tb01.Tag);

			SUT.Model.MyValue = 42;

			Assert.AreEqual(42, SUT.tb01.Tag);
		}

		[TestMethod]
		[RunsOnUIThread]
		public void When_xLoad_xBind_xLoad_While_Loading()
		{
			var grid = new Grid();
			TestServices.WindowHelper.WindowContent = grid;

			var SUT = new When_xLoad_xBind_xLoad_While_Loading();
			grid.Children.Add(SUT);

			Assert.IsNotNull(SUT.tb01);
			Assert.AreEqual(1, SUT.tb01.Tag);

			SUT.Model.MyValue = 42;

			Assert.AreEqual(42, SUT.tb01.Tag);
		}


#if __ANDROID__
		[Ignore("https://github.com/unoplatform/uno/issues/7305")]
#endif
		[TestMethod]
		public async Task When_Binding_xLoad_Nested()
		{
			var SUT = new Binding_xLoad_Nested();
			Assert.IsNull(SUT.tb01);
			Assert.IsNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);

			Assert.AreEqual(0, SUT.TopLevelVisiblity1GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity1SetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity2GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity2SetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity3GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity3SetCount);

			var grid = new Grid();
			TestServices.WindowHelper.WindowContent = grid;
			grid.Children.Add(SUT);

			Assert.IsNull(SUT.tb01);
			Assert.IsNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);
			Assert.AreEqual(2, SUT.TopLevelVisiblity1GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity1SetCount);

			SUT.TopLevelVisiblity1 = true;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);

			SUT.TopLevelVisiblity2 = true;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);

			SUT.TopLevelVisiblity3 = true;

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNotNull(SUT.panel02);
			Assert.IsNotNull(SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			Assert.IsNotNull(SUT.tb06);

			SUT.TopLevelVisiblity3 = false;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNotNull(SUT.panel01);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			await AssertIsNullAsync(() => SUT.tb06);

			SUT.TopLevelVisiblity2 = false;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			// Note: If not null, this usually means that the control is leaking!!!
			await AssertIsNullAsync(() => SUT.panel01);
			await AssertIsNullAsync(() => SUT.tb03);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			await AssertIsNullAsync(() => SUT.tb06);
			await AssertIsNullAsync(() => SUT.panel03);
			await AssertIsNullAsync(() => SUT.tb05);

			SUT.TopLevelVisiblity1 = false;

			await AssertIsNullAsync(() => SUT.tb01);
			await AssertIsNullAsync(() => SUT.tb02);
			await AssertIsNullAsync(() => SUT.panel01);
			await AssertIsNullAsync(() => SUT.tb03);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			await AssertIsNullAsync(() => SUT.panel03);
			await AssertIsNullAsync(() => SUT.tb05);
			await AssertIsNullAsync(() => SUT.tb06);
		}

#if __ANDROID__
		[Ignore("https://github.com/unoplatform/uno/issues/7305")]
#endif
		[TestMethod]
		public async Task When_Binding_xLoad_Nested_With_ElementStub_LoadCount()
		{
			//
			// This test is the same as When_Binding_xLoad_Nested, but with explicit querying
			// of ElementStub instances. This prevents the GC from collecting instances, but allows
			// for counting Load/Unload counts properly.
			//

			var SUT = new Binding_xLoad_Nested();
			Assert.IsNull(SUT.tb01);
			Assert.IsNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);

			Assert.AreEqual(0, SUT.TopLevelVisiblity1GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity1SetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity2GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity2SetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity3GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity3SetCount);

			var grid = new Grid();
			TestServices.WindowHelper.WindowContent = grid;
			grid.Children.Add(SUT);

			Assert.IsNull(SUT.tb01);
			Assert.IsNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);

			Assert.AreEqual(2, SUT.TopLevelVisiblity1GetCount);
			Assert.AreEqual(0, SUT.TopLevelVisiblity1SetCount);

			var tb01Stub = SUT.FindFirstChild<ElementStub>(e => e.Name == "tb01")!;
			var tb02Stub = SUT.FindFirstChild<ElementStub>(e => e.Name == "tb02")!;
			var panel01Stub = SUT.FindFirstChild<ElementStub>(e => e.Name == "panel01")!;
			var panel03Stub = SUT.FindFirstChild<ElementStub>(e => e.Name == "panel03")!;

			var tb01StubChangedCount = 0;
			tb01Stub.MaterializationChanged += _ => tb01StubChangedCount++;

			var tb02StubChangedCount = 0;
			tb02Stub.MaterializationChanged += _ => tb02StubChangedCount++;

			var panel01StubChangedCount = 0;
			panel01Stub.MaterializationChanged += _ => panel01StubChangedCount++;

			var panel03StubChangedCount = 0;
			panel03Stub.MaterializationChanged += _ => panel03StubChangedCount++;

			SUT.TopLevelVisiblity1 = true;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);
			Assert.IsFalse(panel03Stub.Load);
			Assert.AreEqual(1, tb01StubChangedCount);
			Assert.AreEqual(1, tb02StubChangedCount);
			Assert.AreEqual(0, panel01StubChangedCount);

			SUT.TopLevelVisiblity2 = true;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNull(SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			Assert.IsNull(SUT.tb06);
			Assert.IsTrue(panel03Stub.Load);
			Assert.AreEqual(1, tb01StubChangedCount);
			Assert.AreEqual(1, tb02StubChangedCount);
			Assert.AreEqual(1, panel01StubChangedCount);

			var panel02Stub = SUT.FindFirstChild<ElementStub>(e => e.Name == "panel02")!;

			var panel02StubChangedCount = 0;
			panel02Stub.MaterializationChanged += _ => panel02StubChangedCount++;

			SUT.TopLevelVisiblity3 = true;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNotNull(SUT.panel02);
			Assert.IsNotNull(SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			Assert.IsNotNull(SUT.tb06);
			Assert.IsTrue(panel03Stub.Load);
			Assert.AreEqual(1, tb01StubChangedCount);
			Assert.AreEqual(1, tb02StubChangedCount);
			Assert.AreEqual(1, panel01StubChangedCount);
			Assert.AreEqual(1, panel02StubChangedCount);

			SUT.TopLevelVisiblity3 = false;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			Assert.IsNotNull(SUT.tb03);
			Assert.IsNotNull(SUT.panel01);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			Assert.IsNotNull(SUT.tb05);
			await AssertIsNullAsync(() => SUT.tb06);
			Assert.IsTrue(panel03Stub.Load);
			Assert.AreEqual(1, tb01StubChangedCount);
			Assert.AreEqual(1, tb02StubChangedCount);
			Assert.AreEqual(1, panel01StubChangedCount);
			Assert.AreEqual(2, panel02StubChangedCount);

			SUT.TopLevelVisiblity2 = false;

			await Task.Yield();

			Assert.IsNotNull(SUT.tb01);
			Assert.IsNotNull(SUT.tb02);
			await AssertIsNullAsync(() => SUT.panel01);
			await AssertIsNullAsync(() => SUT.tb03);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			await AssertIsNullAsync(() => SUT.tb06);
			Assert.IsFalse(panel03Stub.Load);
			await AssertIsNullAsync(() => SUT.panel03);
			await AssertIsNullAsync(() => SUT.tb05);
			Assert.AreEqual(1, tb01StubChangedCount);
			Assert.AreEqual(1, tb02StubChangedCount);
			Assert.AreEqual(2, panel01StubChangedCount);
			Assert.AreEqual(2, panel02StubChangedCount);

			SUT.TopLevelVisiblity1 = false;

			await Task.Yield();

			await AssertIsNullAsync(() => SUT.tb01);
			await AssertIsNullAsync(() => SUT.tb02);
			await AssertIsNullAsync(() => SUT.panel01);
			await AssertIsNullAsync(() => SUT.tb03);
			await AssertIsNullAsync(() => SUT.panel02);
			await AssertIsNullAsync(() => SUT.tb04);
			Assert.IsFalse(panel03Stub.Load);
			await AssertIsNullAsync(() => SUT.panel03);
			await AssertIsNullAsync(() => SUT.tb05);
			await AssertIsNullAsync(() => SUT.tb06);
			Assert.AreEqual(2, tb01StubChangedCount);
			Assert.AreEqual(2, tb02StubChangedCount);
			Assert.AreEqual(2, panel01StubChangedCount);
			Assert.AreEqual(2, panel02StubChangedCount);
		}

		private async Task AssertIsNullAsync<T>(Func<T> getter, TimeSpan? timeout = null)
		{
			timeout ??= TimeSpan.FromSeconds(10);
			var sw = Stopwatch.StartNew();

			while (sw.Elapsed < timeout && getter() != null)
			{
				await Task.Delay(100);

				// Wait for the ElementNameSubject and ComponentHolder
				// instances to release their references.
				GC.Collect(2);
				GC.WaitForPendingFinalizers();
			}

			Assert.IsNull(getter());
		}
	}
}
#endif
