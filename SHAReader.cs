using System.Security.Cryptography;

namespace CCFileSystem
{
	public class SHAReader : CCReader
	{
		private SHA1 _sha;

		public SHAReader() : base()
		{
			_sha = SHA1.Create();
		}

		public override byte[]? Get(int length)
		{
			byte[]? buffer = base.Get(length);
			if (buffer == null)
				return null;

			_sha.TransformBlock(buffer, 0, buffer.Length, null, 0);
			return buffer;
		}

		public byte[] Result()
		{
			return _sha.TransformFinalBlock(new byte[1], 0, 0);
		}
	}

}