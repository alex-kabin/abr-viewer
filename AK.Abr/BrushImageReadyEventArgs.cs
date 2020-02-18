using System;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
    public class BrushImageReadyEventArgs : EventArgs
    {
        public BitmapSource Bitmap { get; }
        public int Index { get; }

        public BrushImageReadyEventArgs(BitmapSource bitmap, int index) {
            Bitmap = bitmap;
            Index = index;
        }
    }
}