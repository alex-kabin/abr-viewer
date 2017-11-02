using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public class WriteableBitmapImage
	{
		private readonly WriteableBitmap _bitmap;
		private bool _locked;
		private IntPtr _backBuffer;
		private readonly int _bytesPerPixel;
		private int _backBufferStride;
		private readonly object _syncRoot = new object();

		public WriteableBitmapImage(int width, int height)
		{
			_bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
			_bytesPerPixel = _bitmap.Format.BitsPerPixel/8;
			Width = width;
			Height = height;
		}

		public int Width { get; private set; }

		public int Height { get; private set; }

		public void Lock()
		{
			lock (_syncRoot)
			{
				if (!_locked)
				{
					_locked = true;
					_bitmap.Lock();
					_backBuffer = _bitmap.BackBuffer;
					_backBufferStride = _bitmap.BackBufferStride;
				}
			}
		}

		public void Unlock()
		{
			if (_locked)
			{
				lock (_syncRoot)
				{
					if (_locked)
					{
						_bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
						_bitmap.Unlock();
						_locked = false;
					}
				}
			}
		}

		private byte[] ColorToBytes(Color color)
		{
			return new byte[] { color.B, color.G, color.R, color.A };
		}

		private Color ColorFromBytes(params byte[] data)
		{
			return new Color { B = data[0], G = data[1], R = data[2], A = data[3] };
		}

		private Color GetPixelColor(int x, int y)
		{
			if (_locked)
			{
				int offset = (int)(y * _backBufferStride + x * _bytesPerPixel);
				unsafe
				{
					byte* pbuff = (byte*)_backBuffer.ToPointer();
					return ColorFromBytes(pbuff[offset], pbuff[offset + 1], pbuff[offset + 2], pbuff[offset + 3]);
				}
			}
			else
			{
				byte[] pixelBuffer = new byte[_bytesPerPixel];
				Int32Rect rect = new Int32Rect(x, y, 1, 1);
				_bitmap.CopyPixels(rect, pixelBuffer, _bytesPerPixel, 0);
				return ColorFromBytes(pixelBuffer);
			}
		}

		private void SetPixelColor(int x, int y, Color color)
		{
			var colorBytes = ColorToBytes(color);
			if (_locked)
			{
				int offset = (int)(y * _backBufferStride + x * _bytesPerPixel);
				unsafe
				{
					byte* pbuff = (byte*)_backBuffer.ToPointer();
					pbuff[offset] = colorBytes[0];
					pbuff[offset + 1] = colorBytes[1];
					pbuff[offset + 2] = colorBytes[2];
					pbuff[offset + 3] = colorBytes[3];
				}
			}
			else
			{
				Int32Rect rect = new Int32Rect(x, y, 1, 1);
				_bitmap.WritePixels(rect, colorBytes, _bytesPerPixel, 0);
			}
		}

		public Color this[int x, int y]
		{
			get { return GetPixelColor(x, y); }
			set { SetPixelColor(x, y, value); }
		}

		public BitmapSource BitmapSource
		{
			get { return _bitmap; }
		}

		public void Dispose()
		{
			
		}
	}
}