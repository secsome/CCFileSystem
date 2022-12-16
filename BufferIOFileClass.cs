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

		public override bool Open(FileAccess rights = FileAccess.Read)
		{
			try
			{
				if (base.Open(rights))
				{
					if (_stream == null)
						throw new NullReferenceException("_stream");
					
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