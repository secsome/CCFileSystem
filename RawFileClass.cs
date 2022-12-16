namespace CCFileSystem
{
	public class RawFileClass : FileClass
	{
		private FileAccess _rights;
		private long _biasStart;
		private long _biasLength;
		protected Stream? _stream;
		private string _filename;

		public RawFileClass(string filename)
		{
			_rights = FileAccess.Read;
			_biasStart = 0;
			_biasLength = -1;
			_stream = null;
			_filename = filename;
		}

		public RawFileClass()
		{
			_rights = FileAccess.Read;
			_biasStart = 0;
			_biasLength = -1;
			_stream = null;
			_filename = string.Empty;
		}

		~RawFileClass()
		{
			Close();
			_filename = string.Empty;
		}

		public override string File_Name()
		{
			return _filename;
		}

		public override string Set_Name(string filename)
		{
			if (filename.Length == 0)
				return string.Empty;

			Bias(0);

			_filename = filename;
			return _filename;
		}

		public override bool Create()
		{
			Close();

			if (Open(FileAccess.Write))
			{
				if (_biasLength != -1)
					Seek(0, SeekOrigin.Current);

				Close();
				return true;
			}

			return false;
		}

		public override bool Delete()
		{
			Close();

			if (_filename.Length == 0)
				return false;

			File.Delete(_filename);

			bool result = !Open(FileAccess.Read);
			if (!result)
				Close();

			return result;
		}

		public override bool Is_Available(bool forced = false)
		{
			if (_filename.Length == 0)
				return false;

			if (Is_Open())
				return true;

			if (forced)
			{
				OpenImpl(FileAccess.Read);
				CloseImpl();
				return true;
			}

			try
			{
				_stream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				_stream.Dispose();
				_stream = null;
				return true;
			}
			catch { return false; }
		}

		public override bool Is_Open()
		{
			return _stream != null;
		}

		public override bool Open(string filename, FileAccess rights = FileAccess.Read)
		{
			Set_Name(filename);
			return Open(rights);
		}

		private bool OpenImpl(FileAccess rights = FileAccess.Read)
		{
			Close();

			if (_filename.Length == 0)
				return false;

			_rights = rights;

			try
			{
				switch (rights)
				{
					case FileAccess.Read:
						_stream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
						break;
					case FileAccess.Write:
						_stream = new FileStream(_filename, FileMode.Create, FileAccess.Write, FileShare.None);
						break;
					case FileAccess.ReadWrite:
						_stream = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
						break;
					default:
						throw new ArgumentOutOfRangeException("rights");
				}

				if (_biasStart != 0 || _biasLength != -1)
					Seek(0, SeekOrigin.Begin);

				if (_stream == null)
					return false;
			}
			catch { return false; }

			return true;
		}
		public override bool Open(FileAccess rights = FileAccess.Read)
		{
			return OpenImpl(rights);
		}

		public override byte[]? Read(long length)
		{
			try
			{
				bool opened = false;
				if (!Is_Open())
				{
					if (!Open(FileAccess.Read))
						return null;
					opened = true;
				}

				if (_biasLength != -1)
				{
					long remainder = _biasLength - Seek(0);
					if (length > remainder)
						length = remainder;
				}

				var br = new BinaryReader(_stream);
				MemoryStream buffer = new MemoryStream();
				var bw = new BinaryWriter(buffer);
				while (length > 0)
				{
					// 128 MB per time
					int bytesToRead = (int)Math.Min(length, 0x8000000);
					var bytesRead = br.ReadBytes(bytesToRead);
					bw.Write(bytesRead);
					length -= bytesRead.Length;
				}

				if (opened)
					Close();

				return buffer.ToArray();
			}
			catch { return null; }
		}

		public long SeekImpl(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			if (_biasLength != -1)
			{
				switch (dir)
				{
					case SeekOrigin.Begin:
						if (pos > _biasLength)
							pos = _biasLength;

						pos += _biasStart;
						break;
					case SeekOrigin.Current:
						break;
					case SeekOrigin.End:
						dir = SeekOrigin.Begin;
						pos += _biasStart + _biasLength;
						break;
					default:
						throw new ArgumentOutOfRangeException("dir");
				}

				long newpos = RawSeek(pos, dir) - _biasStart;
				if (newpos < 0)
					newpos = RawSeek(_biasStart, SeekOrigin.Begin) - _biasStart;
				if (newpos > _biasLength)
					newpos = RawSeek(_biasStart + _biasLength, SeekOrigin.Begin) - _biasStart;

				return newpos;
			}

			return RawSeek(pos, dir);
		}
		public override long Seek(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			return SeekImpl(pos, dir);
		}

		public long SizeImpl()
		{
			long size = 0;
			if (_biasLength != -1)
				return _biasLength;

			if (Is_Open())
				size = _stream.Length;
			else
			{
				if (Open())
				{
					size = Size();
					Close();
				}
			}

			_biasLength = size - _biasLength;
			return _biasLength;
		}
		public override long Size()
		{
			return SizeImpl();
		}

		public override long Write(byte[] buffer, long size)
		{
			buffer = buffer.LongTake(size);
			try
			{
				bool opened = false;

				if (!Is_Open())
				{
					if (!Open(FileAccess.Write))
						return 0;

					opened = true;
				}

				long length = _stream.Position;
				_stream.Write(buffer, 0, buffer.Length);
				length = _stream.Position - length;

				if (_biasLength != -1)
				{
					if (RawSeek(0) > _biasStart + _biasLength)
						_biasLength = RawSeek(0) - _biasStart;
				}

				if (opened)
					Close();

				return length;
			}
			catch { return 0; }
		}

		private void CloseImpl()
		{
			if (Is_Open())
			{
				_stream.Flush();
				_stream.Dispose();
				_stream = null;
			}
		}
		public override void Close()
		{
			CloseImpl();
		}

		public void Bias(long start, long length = -1)
		{
			if (start == 0)
			{
				_biasStart = 0;
				_biasLength = -1;
				return;
			}

			_biasLength = SizeImpl();
			_biasStart += start;
			if (length != -1 && length < _biasLength)
				_biasLength = length;

			if (_biasLength < 0)
				_biasLength = 0;

			if (Is_Open())
				SeekImpl(0, SeekOrigin.Begin);
		}

		protected long RawSeek(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			return _stream.Seek(pos, dir);
		}

	}
}