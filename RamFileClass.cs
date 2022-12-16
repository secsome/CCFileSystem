namespace CCFileSystem
{
	public class RamFileClass : FileClass
	{
		private byte[] _buffer;
		private int _maxLength;
		private int _length;
		private int _offset;
		private FileAccess _access;
		private bool _isopen;

		public RamFileClass(byte[]? buffer, int len)
		{
			_length = len;
			_maxLength = len;
			_offset = 0;
			_access = FileAccess.Read;
			_isopen = false;
			if (buffer == null)
				_buffer = new byte[len];
			else
				_buffer = buffer;
		}

		~RamFileClass()
		{
			Close();
		}

		public override string FileName()
		{
			return "UNKNOWN";
		}

		public override string SetName(string filename)
		{
			return FileName();
		}

		public override bool Create()
		{
			if (!IsOpen())
			{
				_length = 0;
				return true;
			}
			return false;
		}

		public override bool Delete()
		{
			if (!IsOpen())
			{
				_length = 0;
				return true;
			}
			return false;
		}

		public override bool IsAvailable(bool forced = false)
		{
			return true;
		}

		public override bool IsOpen()
		{
			return _isopen;
		}

		public override bool Open(string filename, FileAccess rights = FileAccess.Read)
		{
			return Open(rights);
		}

		public override bool Open(FileAccess rights = FileAccess.Read)
		{
			if (IsOpen() || _buffer.Length <= 0)
				return false;

			_offset = 0;
			_access = rights;
			_isopen = true;

			switch (rights)
			{
				default:
				case FileAccess.Read:
					break;
				case FileAccess.Write:
					_length = 0;
					break;
				case FileAccess.ReadWrite:
					break;
			}

			return IsOpen();
		}

		public override byte[]? Read(long length)
		{
			if (length <= 0)
				return null;

			bool hasopened = false;

			if (!IsOpen())
			{
				Open();
				hasopened = true;
			}
			else if ((_access & FileAccess.Read) != FileAccess.Read)
				return null;

			int tocopy = (int)Math.Min(length, _length - _offset);
			var ret = new byte[tocopy];
			Buffer.BlockCopy(_buffer, _offset, ret, 0, tocopy);
			_offset += tocopy;

			if (hasopened)
				Close();

			return ret;
		}

		public override long Seek(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			if (!IsOpen())
				return _offset;

			int maxOffset = _length;

			if ((_access & FileAccess.Write) == FileAccess.Write)
				maxOffset = _maxLength;

			switch (dir)
			{
				case SeekOrigin.Begin:
					_offset = (int)pos;
					break;
				case SeekOrigin.Current:
					_offset += (int)pos;
					break;
				case SeekOrigin.End:
					_offset = maxOffset + (int)pos;
					break;
			}

			_offset = Math.Clamp(_offset, 0, maxOffset);
			if (_offset > _length)
				_length = _offset;

			return _offset;
		}

		public override long Size()
		{
			return _length;
		}

		public override long Write(byte[] buffer)
		{
			if (buffer.Length == 0)
				return 0;

			bool hasopened = false;

			if (!IsOpen())
			{
				Open(FileAccess.Write);
				hasopened = true;
			}
			else if ((_access & FileAccess.Write) == FileAccess.Write)
				return 0;

			int towrite = Math.Min(buffer.Length, _maxLength - _offset);
			Buffer.BlockCopy(buffer, 0, _buffer, _offset, towrite);
			_offset += towrite;

			if (_offset > _length)
				_length = _offset;

			if (hasopened)
				Close();

			return towrite;
		}

		public override void Close()
		{
			_isopen = false;
		}
	}
}