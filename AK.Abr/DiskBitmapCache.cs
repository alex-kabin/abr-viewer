using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public class DiskBitmapCache : IBitmapCache
	{
		private readonly string _cacheFolder;
		private readonly bool _isPersistent;
		private bool _isDisposed;
		private readonly object _sync = new object();

		public string CachePath
		{
			get { return _cacheFolder; }
		}

		public override string ToString()
		{
			return "DiskCache ("+CachePath+")";
		}

		public DiskBitmapCache(string cacheFolder = null, bool isPersistent = true)
		{
			_cacheFolder = ResolveCacheFolder(cacheFolder);
			_isPersistent = isPersistent;
			Directory.CreateDirectory(_cacheFolder);
		}

		private string ResolveCacheFolder(string cacheFolder)
		{
			if (String.IsNullOrWhiteSpace(cacheFolder))
				return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

			if (cacheFolder.StartsWith(@".\"))
				return Path.Combine(Environment.CurrentDirectory, cacheFolder.Substring(2));

			return cacheFolder;
		}

		private void ThrowIfDisposed()
		{
			lock (_sync)
			{
				if(_isDisposed)
					throw new ObjectDisposedException("DiskBitmapCache");
			}
		}

		protected virtual BitmapEncoder CreateBitmapEncoder()
		{
			return new PngBitmapEncoder();
		}

		protected virtual string GetIndexFileName(int index)
		{
			return index.ToString() + ".png";
		}

		private string GetGroupDirectoryPath(string key)
		{
			var safeKey = Path.GetInvalidFileNameChars().Aggregate(key, (k, c) => k.Replace(c, '_'));
			return Path.Combine(_cacheFolder, safeKey);
		}

		public bool ContainsKey(string key, long timestamp)
		{
			ThrowIfDisposed();

			var groupDir = GetGroupDirectoryPath(key);
			if (!Directory.Exists(groupDir))
				return false;

			var cacheSlotTimestamp = Directory.GetCreationTime(groupDir).Ticks;
			if (cacheSlotTimestamp < timestamp)
				return false;

			return true;
		}

		public BitmapSource GetBitmap(string key, long timestamp, int index)
		{
			ThrowIfDisposed();

			var groupDir = GetGroupDirectoryPath(key);
			if (!Directory.Exists(groupDir))
				return null;

			var cacheSlotTimestamp = Directory.GetCreationTime(groupDir).Ticks;
			if (cacheSlotTimestamp < timestamp)
				return null;

			var imageFilePath = Path.Combine(groupDir, GetIndexFileName(index));
			if (File.Exists(imageFilePath))
			{
				var image = new BitmapImage(new Uri(imageFilePath, UriKind.Absolute));
				image.Freeze();
				return image;
			}
			
			return null;
		}

		public void PutBitmap(string key, long timestamp, int index, BitmapSource bitmap)
		{
			ThrowIfDisposed();

			var groupDir = GetGroupDirectoryPath(key);
			if (Directory.Exists(groupDir))
			{
				var cacheSlotTimestamp = Directory.GetCreationTime(groupDir).Ticks;
				if (cacheSlotTimestamp < timestamp)
				{
					Directory.Delete(groupDir);
					Directory.CreateDirectory(groupDir);
				}
			}
			else
			{
				Directory.CreateDirectory(groupDir);
			}

			var imageFilePath = Path.Combine(groupDir, GetIndexFileName(index));
			var encoder = CreateBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			using(var fs = File.OpenWrite(imageFilePath))
				encoder.Save(fs);
		}

		public void Dispose()
		{
			lock (_sync)
			{
				if (!_isDisposed)
				{
					if(!_isPersistent)
						try
						{
							Directory.Delete(_cacheFolder, true);
						}
						catch
						{
						}
					_isDisposed = true;
				}
			}
		}
	}
}