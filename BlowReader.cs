namespace CCFileSystem
{
	public class BlowReader : CCReader
	{
		private byte[] _buffer;
		private long _counter;
		private bool _decrypt;
		protected Encryption.Blowfish.BlowfishEcb? _bf;

		public BlowReader(bool decrypt) : base()
		{
			_buffer = new byte[8];
			_counter = 0;
			_decrypt = decrypt;
			_bf = null;
		}

		~BlowReader()
		{
			_bf = null;
		}

		public override byte[]? Get(long length)
		{
			if (length <= 0)
				return null;

			if (_bf == null)
				return base.Get(length);

			long total = 0;
			MemoryStream buffer = new MemoryStream();
			while (length > 0)
			{
				if (_counter > 0)
				{
					long sublen = Math.Min(length, _counter);
					buffer.Write(_buffer, (int)(_buffer.Length - _counter), (int)sublen);
					_counter -= sublen;
					length -= sublen;
					total += sublen;
				}
				if (length > 0)
				{
					var tmp = base.Get(_buffer.Length);
					if (tmp == null)
						break;

					tmp.CopyTo(_buffer, 0);
					if (tmp.Length == _buffer.Length)
					{
						if (_decrypt)
							_bf.Decrypt(_buffer);
						else
							_bf.Encrypt(_buffer);
					}
					else
						_buffer.CopyTo(_buffer, tmp.Length);

					_counter = tmp.Length;
				}
			}

			byte[] arr = buffer.ToArray();
			return arr.Length > 0 ? arr : null;
		}

		public void Key(byte[] key)
		{
			if (_bf == null)
				_bf = new Encryption.Blowfish.BlowfishEcb(key);
		}
	}
}