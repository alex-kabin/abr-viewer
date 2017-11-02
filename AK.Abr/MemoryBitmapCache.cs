using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public class MemoryBitmapCache : IBitmapCache
	{
		private class CacheItem<T>
		{
			public long Timestamp { get; private set; }
			public Dictionary<int, T> Elements { get; private set; }

			public CacheItem(long timestamp)
			{
				Timestamp = timestamp;
				Elements = new Dictionary<int, T>();
			}
		}

		private readonly Dictionary<string, CacheItem<BitmapSource>> _cache = new Dictionary<string, CacheItem<BitmapSource>>();
		private readonly object _sync = new object();
		private readonly int _capacity;
		private bool _isDisposed;

		public MemoryBitmapCache(int capacity)
		{
			_capacity = capacity;
		}

		private void ThrowIfDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException("DiskBitmapCache");
		}

		public bool ContainsKey(string key, long timestamp)
		{
			lock (_sync)
			{
				ThrowIfDisposed();

				if(_cache.ContainsKey(key))
				{
					var item = _cache[key];
					return item.Timestamp >= timestamp;
				}
				return false;
			}
		}

		public BitmapSource GetBitmap(string key, long timestamp, int index)
		{
			BitmapSource image = null;
			lock (_sync)
			{
				ThrowIfDisposed();

				if (_cache.ContainsKey(key))
				{
					var item = _cache[key];
					if (item.Timestamp >= timestamp)
						item.Elements.TryGetValue(index, out image);
				}
			}
			return image;
		}

		private void TrimCache()
		{
			var keyToRemove = _cache.Keys.OrderBy(k => _cache[k].Elements.Count).First();
			_cache.Remove(keyToRemove);
		}

		public void PutBitmap(string key, long timestamp, int index, BitmapSource bitmap)
		{
			lock (_sync)
			{
				ThrowIfDisposed();

				if (_cache.ContainsKey(key))
				{
					var item = _cache[key];
					if (item.Timestamp >= timestamp)
					{
						if (item.Elements.ContainsKey(index))
							item.Elements[index] = bitmap;
						else
							item.Elements.Add(index, bitmap);
					}
					else
					{
						var newItem = new CacheItem<BitmapSource>(DateTime.Now.Ticks);
						newItem.Elements.Add(index, bitmap);
						_cache[key] = newItem;
					}
				}
				else
				{
					var cacheItemsCount = GetItemsCount();
					if (cacheItemsCount + 1 > _capacity)
						TrimCache();

					var newItem = new CacheItem<BitmapSource>(DateTime.Now.Ticks);
					newItem.Elements.Add(index, bitmap);
					_cache.Add(key, newItem);
				}
			}
		}

		private int GetItemsCount()
		{
			lock(_sync)
				return _cache.Keys.Aggregate(0, (i, k) => i + _cache[k].Elements.Count);
		}

		public override string ToString()
		{
			return String.Format("MemoryCache ({0}/{1})", GetItemsCount(), _capacity);
		}
		

		public void Dispose()
		{
			lock (_sync)
			{
				if (!_isDisposed)
				{
					_cache.Clear();
					_isDisposed = true;
				}
			}
		}
	}
}