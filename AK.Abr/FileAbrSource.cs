using System.IO;

namespace AK.Abr
{
	public class FileAbrSource : IAbrSource
	{
		private readonly string _abrFilePath;

		public FileAbrSource(string abrFilePath)
		{
			if (!File.Exists(abrFilePath))
				throw new FileNotFoundException("Can't find brush file", abrFilePath);

			_abrFilePath = abrFilePath;
			Timestamp = File.GetLastWriteTime(_abrFilePath).Ticks;
		}

		public string Name
		{
			get { return _abrFilePath; }
		}

		public long Timestamp
		{
			get; private set;
		}

		public Stream OpenRead()
		{
			return File.OpenRead(_abrFilePath);
		}
	}
}