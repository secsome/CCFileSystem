namespace CCFileSystem
{
	public class BufferIOFileClass : RawFileClass
	{
		public BufferIOFileClass(string filename) : base(filename)
		{
		}

		public BufferIOFileClass() : base()
		{
		}

		public new bool Open(FileAccess rights = FileAccess.Read)
		{
			try
			{
				if ((this as RawFileClass).Open(rights))
			{
				_stream = new BufferedStream(_stream);
				return true;
			}
			else
				return false;
			}
			catch { return false; }
		}
	}
}