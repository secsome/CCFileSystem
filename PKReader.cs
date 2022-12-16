namespace CCFileSystem
{
	public class PKReader : CCReader
	{
		public static readonly int BLOWFISH_KEY_SIZE = 56;

		private bool _isGettingKey;
		private BlowReader _bf;
		private bool _decrypt;
		private PKey? _cipherKey;
		private byte[] _buffer;
		private long _counter;
		private long _bytesLeft;

		public PKReader(bool decrypt) : base()
		{
			_isGettingKey = true;
			_bf = new BlowReader(decrypt);
			_decrypt = decrypt;
			_cipherKey = null;
			_buffer = new byte[256];
			_counter = 0;
			_bytesLeft = 0;
		}

		~PKReader()
		{
			_cipherKey = null;
		}

		public override void Get_From(CCReader? reader)
		{
			if (_bf.ChainTo != reader)
			{
				if (reader != null && reader.ChainFrom != null)
				{
					reader.ChainFrom.Get_From(null);
					reader.ChainFrom = null;
				}

				if (_bf.ChainTo != null)
					_bf.ChainTo.ChainFrom = null;

				_bf.ChainTo = reader;
				_bf.ChainFrom = this;
				ChainTo = _bf;
				if (_bf.ChainTo != null)
					_bf.ChainTo.ChainFrom = this;
			}
		}

		public override byte[]? Get(long length)
		{
			if (_cipherKey == null || length < 1)
				return base.Get(length);

			MemoryStream buffer = new MemoryStream();
			if (_isGettingKey)
			{
				if (_decrypt)
				{
					var cbuffer = base.Get(Encrypted_Key_Length);
					if (cbuffer == null || cbuffer.Length != Encrypted_Key_Length)
						return null;

					var t = _cipherKey.Decrypt(cbuffer);
					t.CopyTo(_buffer, 0);
					_bf.Key(_buffer.Take(BLOWFISH_KEY_SIZE).ToArray());
				}
				else
				{
					// TODO
					throw new NotImplementedException();
					byte[] cbuffer = new byte[BLOWFISH_KEY_SIZE];
					_buffer = _cipherKey.Encrypt(cbuffer);
					_counter = _bytesLeft = _buffer.Length;
					_bf.Key(_buffer);
				}

				_isGettingKey = false;
			}

			if (_bytesLeft > 0)
			{
				long tocopy = Math.Min(length, _bytesLeft);
				buffer.Write(_buffer.Skip((int)(_counter - _bytesLeft)).Take((int)tocopy).ToArray());
				_bytesLeft -= tocopy;
				length -= tocopy;
			}

			var tmp = base.Get(length);
			if (tmp == null)
				return null;

			return buffer.ToArray().Concat(tmp).ToArray();
		}

		public void Key(PKey? key)
		{
			_cipherKey = key;
			if (key != null)
				_isGettingKey = true;

			_counter = 0;
			_bytesLeft = 0;
		}

		private long Encrypted_Key_Length
		{
			get
			{
				if (_cipherKey == null)
					return 0;
				return _cipherKey.Block_Count(BLOWFISH_KEY_SIZE) * _cipherKey.Crypt_Block_Size;
			}
		}

		private long Plain_Key_Length
		{
			get
			{
				if (_cipherKey == null)
					return 0;
				return _cipherKey.Block_Count(BLOWFISH_KEY_SIZE) * _cipherKey.Plain_Block_Size;
			}
		}
	}
}