namespace CCFileSystem
{
	public class FileReader : CCReader
	{
		private FileClass? _file;
		private bool _hasOpened;

		public FileReader(FileClass? file) : base()
		{
			_file = file;
			_hasOpened = false;
		}

		~FileReader()
		{
			if (_file != null && _hasOpened)
			{
				_file.Close();
				_hasOpened = false;
				_file = null;
			}
		}

		public override byte[]? Get(int length)
		{
			if (_file != null && length > 0)
			{
				if (!_file.Is_Open())
				{
					_hasOpened = true;

					if (!_file.Is_Available())
						return null;

					if (!_file.Open(FileAccess.Read))
						return null;
				}

				return _file.Read(length);
			}

			return null;
		}
	}

}