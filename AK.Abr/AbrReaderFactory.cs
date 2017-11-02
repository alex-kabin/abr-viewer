namespace AK.Abr
{
	public class AbrReaderFactory : IAbrReaderFactory
	{
		public IAbrReader GetReader(IAbrSource source)
		{
			return new AbrReader(source);
		}

		public override string ToString()
		{
			return "AbrReaderFactory";
		}
	}
}