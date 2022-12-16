namespace CCFileSystem
{
	public abstract class CCWriter
	{
		private CCWriter? _to;
		private CCWriter? _from;

		public CCWriter()
		{
			_to = null;
			_from = null;
		}

		int Flush()
		{
			if (_to == null)
				return 0;
			return _to.Flush();
		}

		int End()
		{
			return Flush();
		}

		void PutTo(CCWriter? writer)
		{
			if (_to != writer)
			{
				if (writer != null && writer._from != null)
				{
					writer._from.PutTo(null);
					writer._from = null;
				}

				if (_to != null)
				{
					_to._from = null;
					_to.Flush();
				}

				_to = writer;
				if (_to != null)
					_to._from = this;
			}
		}

		int Put(byte[] source)
		{
			if (_to == null)
				return source.Length;
			return _to.Put(source);
		}
	}
}