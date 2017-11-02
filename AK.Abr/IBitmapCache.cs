using System;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public interface IBitmapCache : IDisposable
	{
		bool ContainsKey(string key, long timestamp);
		BitmapSource GetBitmap(string key, long timestamp, int index);
		void PutBitmap(string key, long timestamp, int index, BitmapSource bitmap);
	}
}