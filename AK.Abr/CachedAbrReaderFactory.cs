namespace AK.Abr
{
	public class CachedAbrReaderFactory : IAbrReaderFactory
	{
		private readonly IAbrReaderFactory _factory;
		private readonly IBitmapCache _cache;

		public CachedAbrReaderFactory(IAbrReaderFactory factory, IBitmapCache cache)
		{
			_factory = factory;
			_cache = cache;
		}

		public IAbrReader GetReader(IAbrSource source)
		{
			return new CachedAbrReader(_factory.GetReader(source), _cache);
		}

		public override string ToString()
		{
			return string.Format("CachedAbrReaderFactory [Cache={0}]", _cache.ToString());
		}
	}
}