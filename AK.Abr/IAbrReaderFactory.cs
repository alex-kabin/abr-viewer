namespace AK.Abr
{
	public interface IAbrReaderFactory
	{
		IAbrReader GetReader(IAbrSource source);
	}
}