namespace CCFileSystem
{
	public class CCBuffer
	{
		private byte[] _buffer;
		private long _length;
		private long _beginOffset;

		public CCBuffer(byte[] buffer, long length, long offset = 0)
		{
			_buffer = buffer;
			_length = length;
			_beginOffset = offset;
		}

		public byte this[long idx]
		{
			get => _buffer[_beginOffset + idx];
			set => _buffer[_beginOffset + idx] = value;
		}

		public long Length => _length;

		public byte[] RawBuffer => _buffer;

		public long BeginOffset => _beginOffset;
	}
}