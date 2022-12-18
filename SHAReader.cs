using System.Security.Cryptography;

namespace CCFileSystem
{
	public class SHAReader : CCReader
	{
		private SHA1 _sha;
		private byte[]? _buffer;

		public SHAReader() : base()
		{
			_sha = SHA1.Create();
		}

		public override byte[]? Get(long length)
		{
			if (_buffer != null)
				_sha.TransformBlock(_buffer, 0, _buffer.Length, null, 0);

			_buffer = base.Get(length);
			return _buffer;
		}

		public byte[]? Result()
		{
			if (_buffer == null)
				return null;
			_sha.TransformFinalBlock(_buffer, 0, _buffer.Length);
			return _sha.Hash;
		}
	}

}