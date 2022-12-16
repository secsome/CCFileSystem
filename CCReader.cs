namespace CCFileSystem
{
	public abstract class CCReader
	{
		private CCReader? _to;
		private CCReader? _from;

		public CCReader()
		{
			_to = null;
			_from = null;
		}

		public virtual void Get_From(CCReader? reader)
		{
			if (_to != reader)
			{
				if (reader != null && reader._from != null)
				{
					reader._from.Get_From(null);
					reader._from = null;
				}

				if (_to != null)
					_to._from = null;

				_to = reader;
				if (_to != null)
					_to._from = this;
			}
		}

		public virtual byte[]? Get(long length)
		{
			if (_to == null)
				return null;

			return _to.Get(length);
		}
	}
}