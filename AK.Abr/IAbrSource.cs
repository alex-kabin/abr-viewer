using System.IO;

namespace AK.Abr
{
	public interface IAbrSource
	{
		string Name { get; }
		long Timestamp { get; }
		Stream OpenRead();
	}
}