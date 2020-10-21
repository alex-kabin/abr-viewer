using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AK.Abr
{
    public class CachingAbrReader : IAbrReader, IDisposable
    {
        private readonly IAbrReader _reader;
        private readonly IBitmapCache _cache;

        public IAbrSource Source => _reader.Source;

        public CachingAbrReader(IAbrReader reader, IBitmapCache cache) {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _reader.BrushImageReady += _reader_BrushImageReady; 
        }

        private void _reader_BrushImageReady(object sender, BrushImageReadyEventArgs e)
        {
            _cache.PutBitmap(Source.Name, Source.Timestamp, e.Index, e.Bitmap);
            OnBrushImageReady(e.Bitmap, e.Index);
        }

        public Task ReadAsync(CancellationToken cancellationToken) {
            if (_cache.ContainsKey(Source.Name, Source.Timestamp)) {
                return Task.Factory.StartNew(
                    () => ReadCached(cancellationToken),
                    cancellationToken
                );
            }

            return _reader.ReadAsync(cancellationToken);
        }

        private void ReadCached(CancellationToken cancellationToken) {
            int index = 0;
            BitmapSource cachedImage;
            do {
                cancellationToken.ThrowIfCancellationRequested();

                cachedImage = _cache.GetBitmap(Source.Name, Source.Timestamp, index);
                if (cachedImage != null) {
                    OnBrushImageReady(cachedImage, index);
                    index++;
                }
            } while (cachedImage != null);
        }
        
        public event EventHandler<BrushImageReadyEventArgs> BrushImageReady;

        private void OnBrushImageReady(BitmapSource bitmap, int index) {
            BrushImageReady?.Invoke(this, new BrushImageReadyEventArgs(bitmap, index));
        }

        public void Dispose() {
            if (_reader != null) {
                _reader.BrushImageReady -= _reader_BrushImageReady;
            }
        }
    }
}