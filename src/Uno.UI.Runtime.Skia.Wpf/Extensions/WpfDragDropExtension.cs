﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.DragDrop;
using Windows.ApplicationModel.DataTransfer.DragDrop.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Uno.Extensions;
using DragEventArgs = System.Windows.DragEventArgs;
using Point = Windows.Foundation.Point;
using UIElement = Microsoft.UI.Xaml.UIElement;
using WpfWindow = System.Windows.Window;
using WpfControl = System.Windows.Controls.Control;
using WpfApplication = System.Windows.Application;
using Uno.Foundation.Logging;
using Uno.UI.Runtime.Skia.Wpf;

namespace Uno.UI.Runtime.Skia.Wpf
{
	internal class WpfDragDropExtension : IDragDropExtension
	{
		private readonly long _fakePointerId = Pointer.CreateUniqueIdForUnknownPointer();
		private readonly DragDropManager _manager;

		private static WpfControl? _rootControl;

		public WpfDragDropExtension(object owner)
		{
			_manager = (DragDropManager)owner;

			WpfManager.XamlRootMap.Registered += XamlRootMap_Registered;
		}

		private void XamlRootMap_Registered(object? sender, XamlRoot xamlRoot)
		{
			// TODO:MZ: Multi-window support
			var host = WpfManager.XamlRootMap.GetHostForRoot(xamlRoot) as WpfControl;
			_rootControl = host;

			if (host is not null) // TODO: Add support for multiple XamlRoots
			{
				host.AllowDrop = true;

				host.DragEnter += OnHostDragEnter;
				host.DragOver += OnHostDragOver;
				host.DragLeave += OnHostDragLeave;
				host.Drop += OnHostDrop;
			}
		}

		private void OnHostDragEnter(object sender, DragEventArgs e)
		{
			var src = new DragEventSource(_fakePointerId, e);
			var data = ToDataPackage(e.Data);
			var allowedOperations = ToDataPackageOperation(e.AllowedEffects);
			var info = new CoreDragInfo(src, data.GetView(), allowedOperations);

			CoreDragDropManager.GetForCurrentView()?.DragStarted(info);

			// Note: No needs to _manager.ProcessMove, the DragStarted will actually have the same effect
		}

		private void OnHostDragOver(object sender, DragEventArgs e)
			=> e.Effects = ToDropEffects(_manager.ProcessMoved(new DragEventSource(_fakePointerId, e)));

		private void OnHostDragLeave(object sender, DragEventArgs e)
			=> e.Effects = ToDropEffects(_manager.ProcessAborted(new DragEventSource(_fakePointerId, e)));

		private void OnHostDrop(object sender, DragEventArgs e)
			=> e.Effects = ToDropEffects(_manager.ProcessDropped(new DragEventSource(_fakePointerId, e)));

		public void StartNativeDrag(CoreDragInfo info)
		{
			if (_rootControl is null)
			{
				if (this.Log().IsEnabled(LogLevel.Error))
				{
					this.Log().LogError("Can't start dragging until root element is initialized");
				}
				return;
			}

			_rootControl.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					var data = await ToDataObject(info.Data, CancellationToken.None);
					var effects = ToDropEffects(info.AllowedOperations);

					DragDrop.DoDragDrop(_rootControl, data, effects);
				}
				catch (Exception e)
				{
					this.Log().Error("Failed to start native Drag and Drop.", e);
				}
			});
		}

		private static DataPackageOperation ToDataPackageOperation(DragDropEffects wpfOp)
			=> (DataPackageOperation)((int)wpfOp) & (DataPackageOperation.Copy | DataPackageOperation.Move | DataPackageOperation.Link);

		private static DragDropEffects ToDropEffects(DataPackageOperation uwpOp)
			=> (DragDropEffects)((int)uwpOp) & (DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

		private static DataPackage ToDataPackage(IDataObject src)
		{
			var dst = new DataPackage();
			var text = src.GetData(DataFormats.Text) as string
				?? src.GetData(DataFormats.UnicodeText) as string
				?? src.GetData(DataFormats.OemText) as string
				?? src.GetData(DataFormats.StringFormat) as string;

			if (!text.IsNullOrWhiteSpace())
			{
				if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
				{
					DataPackage.SeparateUri(
						text,
						out string? webLink,
						out string? applicationLink);

					if (webLink is not null)
					{
						dst.SetWebLink(new Uri(webLink));
					}

					if (applicationLink is not null)
					{
						dst.SetApplicationLink(new Uri(applicationLink));
					}

					// Deprecated but still added for compatibility
					dst.SetUri(new Uri(text));
				}
				else
				{
					dst.SetText(text!);
				}
			}

			if (src.GetData(DataFormats.Html) is string html)
			{
				dst.SetHtmlFormat(html);
			}

			if (src.GetData(DataFormats.Rtf) is string rtf)
			{
				dst.SetRtf(rtf);
			}

			if (src.GetData(DataFormats.Bitmap) is BitmapSource bitmap)
			{
				dst.SetBitmap(new RandomAccessStreamReference(ct =>
				{
					var copy = new MemoryStream();
					var encoder = new BmpBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(bitmap));
					encoder.Save(copy);
					copy.Position = 0;

					return Task.FromResult(copy.AsRandomAccessStream().TrySetContentType("image/bmp"));
				}));
			}

			if (src.GetData(DataFormats.FileDrop) is string[] files)
			{
				dst.SetStorageItems(files.Select(StorageFile.GetFileFromPath));
			}

			return dst;
		}

		private static async Task<DataObject> ToDataObject(DataPackageView src, CancellationToken ct)
		{
			var dst = new DataObject();

			// WPF has no format for URI therefore text is used for both
			if (src.Contains(StandardDataFormats.Text))
			{
				dst.SetText(await src.GetTextAsync().AsTask(ct));
			}
			else
			{
				var uri = DataPackage.CombineUri(
					src.Contains(StandardDataFormats.WebLink) ? (await src.GetWebLinkAsync().AsTask(ct)).ToString() : null,
					src.Contains(StandardDataFormats.ApplicationLink) ? (await src.GetApplicationLinkAsync().AsTask(ct)).ToString() : null,
					src.Contains(StandardDataFormats.Uri) ? (await src.GetUriAsync().AsTask(ct)).ToString() : null);

				if (string.IsNullOrEmpty(uri) == false)
				{
					dst.SetText(uri);
				}
			}

			if (src.Contains(StandardDataFormats.Html))
			{
				dst.SetData(DataFormats.Html, await src.GetHtmlFormatAsync().AsTask(ct));
			}

			if (src.Contains(StandardDataFormats.Rtf))
			{
				dst.SetData(DataFormats.Rtf, await src.GetRtfAsync().AsTask(ct));
			}

			if (src.Contains(StandardDataFormats.Bitmap))
			{
				var srcStreamRef = await src.GetBitmapAsync().AsTask(ct);
				var srcStream = await srcStreamRef.OpenReadAsync().AsTask(ct);

				// We copy the source stream in memory to avoid marshalling issue with async stream
				// and to make sure to read it async as it built from a RandomAccessStream which might be remote.
				using var tmp = new MemoryStream();
				await srcStream.AsStreamForRead().CopyToAsync(tmp);
				tmp.Position = 0;

				var dstBitmap = new BitmapImage();
				dstBitmap.BeginInit();
				dstBitmap.CreateOptions = BitmapCreateOptions.None;
				dstBitmap.CacheOption = BitmapCacheOption.OnLoad; // Required for the BitmapImage to internally cache the data (so we can dispose the tmp stream)
				dstBitmap.StreamSource = tmp;
				dstBitmap.EndInit();

				dst.SetData(DataFormats.Bitmap, dstBitmap, false);
			}

			if (src.Contains(StandardDataFormats.StorageItems))
			{
				var files = await src.GetStorageItemsAsync().AsTask(ct);
				var paths = new StringCollection();
				foreach (var item in files)
				{
					paths.Add(item.Path);
				}

				dst.SetFileDropList(paths);
			}

			return dst;
		}

		private class DragEventSource : IDragEventSource
		{
			private readonly DragEventArgs _wpfArgs;
			private static long _nextFrameId;

			public DragEventSource(long pointerId, DragEventArgs wpfArgs)
			{
				_wpfArgs = wpfArgs;
				Id = pointerId;
			}

			public long Id { get; }

			public uint FrameId { get; } = (uint)Interlocked.Increment(ref _nextFrameId);

			/// <inheritdoc />
			public (Point location, DragDropModifiers modifier) GetState()
			{
				var wpfLocation = _wpfArgs.GetPosition(_rootControl);
				var location = new Windows.Foundation.Point(wpfLocation.X, wpfLocation.Y);

				var mods = DragDropModifiers.None;
				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.LeftMouseButton))
				{
					mods |= DragDropModifiers.LeftButton;
				}
				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.MiddleMouseButton))
				{
					mods |= DragDropModifiers.MiddleButton;
				}
				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.RightMouseButton))
				{
					mods |= DragDropModifiers.RightButton;
				}

				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
				{
					mods |= DragDropModifiers.Shift;
				}
				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
				{
					mods |= DragDropModifiers.Control;
				}
				if (_wpfArgs.KeyStates.HasFlag(DragDropKeyStates.AltKey))
				{
					mods |= DragDropModifiers.Alt;
				}

				return (location, mods);
			}

			/// <inheritdoc />
			public Point GetPosition(object? relativeTo)
			{
				var rawWpfPosition = _wpfArgs.GetPosition(_rootControl);
				var rawPosition = new Point(rawWpfPosition.X, rawWpfPosition.Y);

				if (relativeTo is null)
				{
					return rawPosition;
				}

				if (relativeTo is UIElement elt)
				{
					var eltToRoot = UIElement.GetTransform(elt, null);
					var rootToElt = eltToRoot.Inverse();

					return rootToElt.Transform(rawPosition);
				}

				throw new InvalidOperationException("The relative to must be a UIElement.");
			}
		}
	}
}
