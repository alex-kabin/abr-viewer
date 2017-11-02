using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace AK.Abr
{
	public class AbrReader : IAbrReader
	{
		private readonly IAbrSource _source;
		private CancellationToken _cancellationToken;

		public AbrReader(IAbrSource source)
		{
			if(source == null)
				throw new ArgumentNullException("source");

			_source = source;
		}

		private void ReadCore()
		{
			var converter = new BigEndianBitConverter();
			using (var fs = _source.OpenRead())
			{
				var ebr = new EndianBinaryReader(converter, fs);

				int ver = ebr.ReadInt16();
				switch (ver)
				{
					case 1:
						this.ReadVer12(ebr, ver);
						break;

					case 2:
						this.ReadVer12(ebr, ver);
						break;

					case 6:
						this.ReadVer6(ebr);
						break;

					default:
						throw new NotSupportedException("Unsupported file version");
				}
			}
		}

		private static byte[] Unpack(byte[] imgdata)
		{
			using (var input = new MemoryStream(imgdata))
			using (var output = new MemoryStream(imgdata.Length))
			{
				var reader = new BinaryReader(input);
				var writer = new BinaryWriter(output);

				var length = imgdata.Length - sizeof(byte);
				while (input.Position < length)
				{
					sbyte count = reader.ReadSByte();
					if (count >= 0)
					{
						writer.Write(reader.ReadBytes(count+1));
					}
					else
					{
						byte value = reader.ReadByte();
						while(count++ <= 0)
							writer.Write(value);
					}
				}
				return output.ToArray();
			}
		}

		private BitmapSource CreateImage(int width, int height, byte[] buffer)
		{
			var bitmap = new WriteableBitmapImage(width, height);
			bitmap.Lock();

			try
			{
				int index = 0;
				for (int y = 0; y < height; y++)
				{
					_cancellationToken.ThrowIfCancellationRequested();
					for (int x = 0; x < width; x++)
					{
						bitmap[x, y] = new Color() { A = buffer[index++] };
					}
				}
			}
			finally
			{
				bitmap.Unlock();
			}

			var source = bitmap.BitmapSource;
			source.Freeze();

			return source;
		}

		private void ReadVer12(EndianBinaryReader ebr, int ver)
		{
			int num = ebr.ReadInt16();
			for (int i = 0; i < num; i++)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				int num3 = ebr.ReadInt16();
				int num4 = ebr.ReadInt32();
				switch (num3)
				{
					case 1:
						if (ver == 1)
						{
							ebr.ReadBytes(14);
						}
						if (ver == 2)
						{
							ebr.ReadBytes(num4);
						}
						break;

					case 2:
						{
							ebr.ReadInt32();
							ebr.ReadInt16();
							if (ver == 1)
							{
								ebr.ReadByte();
							}
							if (ver == 2)
							{
								int num5 = ebr.ReadInt32();
								ebr.ReadBytes(num5 * 2);
								ebr.ReadBytes(1);
							}
							ebr.ReadInt16();
							ebr.ReadInt16();
							ebr.ReadInt16();
							ebr.ReadInt16();
							int num6 = ebr.ReadInt32();
							int num7 = ebr.ReadInt32();
							int num8 = ebr.ReadInt32();
							int num9 = ebr.ReadInt32();
							ebr.ReadInt16();
							int num10 = ebr.ReadByte();
							int width = num9 - num7;
							int height = num8 - num6;

							byte[] buffer;
							if (num10 == 0)
							{
								buffer = ebr.ReadBytes(width * height);
							}
							else
							{
								int num13 = 0;
								for (int k = 0; k < height; k++)
								{
									num13 += ebr.ReadInt16();
								}

								byte[] imgdata = ebr.ReadBytes(num13);
								buffer = Unpack(imgdata);
							}

							var image = CreateImage(width, height, buffer);
							OnBrushImageReady(image, i);

							break;
						}
				}
			}
		}

		private void ReadVer6(EndianBinaryReader ebr)
		{
			int width = 0;
			int height = 0;
			int num3 = ebr.ReadInt16();
			ebr.ReadBytes(8);
			int num5 = ebr.ReadInt32() + 12;
			int index = 0;
			while (ebr.BaseStream.Position < (num5 - 1))
			{
				_cancellationToken.ThrowIfCancellationRequested();

				int num6 = ebr.ReadInt32();
				int num7 = num6;
				while ((num7 % 4) != 0)
					num7++;

				int num8 = num7 - num6;
				ebr.ReadString();

				switch (num3)
				{
					case 1:
						ebr.ReadInt16();
						ebr.ReadInt16();
						ebr.ReadInt16();
						ebr.ReadInt16();
						ebr.ReadInt16();
						int num9 = ebr.ReadInt32();
						int num10 = ebr.ReadInt32();
						int num11 = ebr.ReadInt32();
						width = ebr.ReadInt32() - num10;
						height = num11 - num9;
						break;
					case 2:
						ebr.ReadBytes(0x108);
						int num13 = ebr.ReadInt32();
						int num14 = ebr.ReadInt32();
						int num15 = ebr.ReadInt32();
						width = ebr.ReadInt32() - num14;
						height = num15 - num13;
						break;
				}

				ebr.ReadInt16();

				byte[] buffer;
				if (ebr.ReadByte() == 0)
				{
					buffer = ebr.ReadBytes(width * height);
				}
				else
				{
					int num18 = 0;
					for (int j = 0; j < height; j++)
						num18 += ebr.ReadInt16();

					byte[] imgdata = ebr.ReadBytes(num18);
					buffer = Unpack(imgdata);
				}

				var image = CreateImage(width, height, buffer);
				OnBrushImageReady(image, index);

				index++;

				switch (num3)
				{
					case 1:
						ebr.ReadBytes(num8);
						continue;
					case 2:
						ebr.ReadBytes(8);
						ebr.ReadBytes(num8);
						break;
				}
			}
		}

		public Task ReadAsync(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
			return Task.Factory.StartNew(ReadCore, cancellationToken);
		}

		public event EventHandler<BrushImageReadyEventArgs> BrushImageReady;
		public IAbrSource Source
		{
			get { return _source; }
		}

		private void OnBrushImageReady(BitmapSource bitmap, int index)
		{
			if (BrushImageReady != null)
			{
				BrushImageReady(this, new BrushImageReadyEventArgs(bitmap, index));
			}
		}
	}
}