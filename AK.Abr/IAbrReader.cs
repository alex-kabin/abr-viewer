using System;
using System.Threading;
using System.Threading.Tasks;

namespace AK.Abr
{
	public interface IAbrReader
	{
		Task ReadAsync(CancellationToken cancellationToken);
		event EventHandler<BrushImageReadyEventArgs> BrushImageReady;
		IAbrSource Source { get; }
	}
}