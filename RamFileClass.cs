namespace CCFileSystem
{
	public class RamFileClass : FileClass
	{
		private byte[] _buffer;
		private long _maxLength;
		private long _length;
		private long _offset;
		private FileAccess _access;
		private bool _isopen;

		public RamFileClass(byte[]? buffer, long len)
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

		public override string File_Name()
		{
			return "UNKNOWN";
		}

		public override string Set_Name(string filename)
		{
			return File_Name();
		}

		public override bool Create()
		{
			if (!Is_Open())
			{
				_length = 0;
				return true;
			}
			return false;
		}

		public override bool Delete()
		{
			if (!Is_Open())
			{
				_length = 0;
				return true;
			}
			return false;
		}

		public override bool Is_Available(bool forced = false)
		{
			return true;
		}

		public override bool Is_Open()
		{
			return _isopen;
		}

		public override bool Open(string filename, FileAccess rights = FileAccess.Read)
		{
			return Open(rights);
		}

		public override bool Open(FileAccess rights = FileAccess.Read)
		{
			if (Is_Open() || _buffer.Length <= 0)
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

			return Is_Open();
		}

		public override byte[]? Read(long length)
		{
			if (length <= 0)
				return null;

			bool hasopened = false;

			if (!Is_Open())
			{
				Open();
				hasopened = true;
			}
			else if ((_access & FileAccess.Read) != FileAccess.Read)
				return null;

			long tocopy = Math.Min(length, _length - _offset);
			var ret = new byte[tocopy];
			ret.MemCopy(_offset, ret, 0, tocopy);
			_offset += tocopy;

			if (hasopened)
				Close();

			return ret;
		}

		public override long Seek(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			if (!Is_Open())
				return _offset;

			long maxOffset = _length;

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

		public override long Write(byte[] buffer, long size)
		{
			buffer = buffer.LongTake(size);

			if (buffer.Length == 0)
				return 0;

			bool hasopened = false;

			if (!Is_Open())
			{
				Open(FileAccess.Write);
				hasopened = true;
			}
			else if ((_access & FileAccess.Write) == FileAccess.Write)
				return 0;

			long towrite = Math.Min(buffer.Length, _maxLength - _offset);
			buffer.MemCopy(0, _buffer, _offset, towrite);
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