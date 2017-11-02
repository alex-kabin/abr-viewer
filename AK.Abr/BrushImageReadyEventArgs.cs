using System;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
	public class BrushImageReadyEventArgs : EventArgs
	{
		public BitmapSource Bitmap { get; private set; }
		public int Index { get; private set; }
		
		public BrushImageReadyEventArgs(BitmapSource bitmap, int index)
		{
			Bitmap = bitmap;
			Index = index;
		}
	}
}