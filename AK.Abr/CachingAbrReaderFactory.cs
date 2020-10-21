namespace AK.Abr
{
    public class CachingAbrReaderFactory : IAbrReaderFactory
    {
        private readonly IAbrReaderFactory _factory;
        private readonly IBitmapCache _cache;

        public CachingAbrReaderFactory(IAbrReaderFactory factory, IBitmapCache cache) {
            _factory = factory;
            _cache = cache;
        }

        public IAbrReader GetReader(IAbrSource source) {
            return new CachingAbrReader(_factory.GetReader(source), _cache);
        }

        public override string ToString() {
            return $"CachingAbrReaderFactory [Cache={_cache.ToString()}]";
        }
    }
}