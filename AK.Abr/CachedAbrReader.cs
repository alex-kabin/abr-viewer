using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public class CachedAbrReader : IAbrReader
	{
		private readonly IAbrReader _reader;
		private readonly IBitmapCache _cache;

		public CachedAbrReader(IAbrReader reader, IBitmapCache cache)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			if (cache == null)
				throw new ArgumentNullException("cache");

			_reader = reader;
			_reader.BrushImageReady += (s, e) =>
			                           	{
			                           		_cache.PutBitmap(Source.Name, Source.Timestamp, e.Index, e.Bitmap);
			                           		OnBrushImageReady(e.Bitmap, e.Index);
			                           	};

			_cache = cache;
		}

		public Task ReadAsync(CancellationToken cancellationToken)
		{
			if (_cache.ContainsKey(Source.Name, Source.Timestamp))
				return Task.Factory.StartNew(
					delegate
						{
							ReadCached(cancellationToken);
						},
					cancellationToken);

			return _reader.ReadAsync(cancellationToken);
		}

		private void ReadCached(CancellationToken cancellationToken)
		{
			int index = 0;
			BitmapSource cachedImage;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				cachedImage = _cache.GetBitmap(Source.Name, Source.Timestamp, index);
				if (cachedImage != null)
				{
					OnBrushImageReady(cachedImage, index);
					index++;
				}
			} while (cachedImage != null);
		}

		public IAbrSource Source
		{
			get { return _reader.Source; }
		}

		public event EventHandler<BrushImageReadyEventArgs> BrushImageReady;
		
		private void OnBrushImageReady(BitmapSource bitmap, int index)
		{
			if (BrushImageReady != null)
			{
				BrushImageReady(this, new BrushImageReadyEventArgs(bitmap, index));
			}
		}
	}
}