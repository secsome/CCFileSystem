namespace CCFileSystem
{
	public class CCFileClass : CDFileClass
	{
		private byte[]? _data;
		private long _position;

		public CCFileClass(string filename) : base(filename)
		{
			_position = 0;
			Set_Name(filename);
		}

		public CCFileClass() : base()
		{
			_position = 0;
		}

		public override long Write(byte[] buffer, long size)
		{
			if (_data != null)
				return 0;

			return base.Write(buffer, size);
		}

		public override byte[]? Read(long size)
		{
			bool opened = false;

			if (!Is_Open())
			{
				if (Open())
					opened = true;
			}

			if (_data != null)
			{
				size = Math.Min(size, _data.LongLength - _position);
				byte[]? ret = null;
				if (size > 0)
				{
					ret=  new byte[size];
					_data.MemCopy(_position, ret, 0, size);
					_position += size;
				}
				if (opened)
					Close();
				return ret;
			}

			var s = base.Read(size);
			if (opened)
				Close();
			return s;
		}

		public override long Seek(long pos, SeekOrigin dir = SeekOrigin.Current)
		{
			if (_data != null)
			{
				switch (dir)
				{
					case SeekOrigin.Begin:
						_position = 0;
						break;
					case SeekOrigin.Current:
						break;
					case SeekOrigin.End:
						_position = _data.LongLength;
						break;
					default:
						throw new ArgumentOutOfRangeException("dir");
				}
				_position += pos;
				_position = Math.Clamp(_position, 0, _data.LongLength);

				return _position;
			}

			return base.Seek(pos, dir);
		}

		public override long Size()
		{
			if (_data != null)
				return _data.LongLength;
			
			if (!base.Is_Available())
			{
				byte[]? data;
				int offset;
				int size;
				MFCC.Offset(File_Name(), out data, out offset, out size);
				return size;
			}

			return base.Size();
		}

		public override bool Is_Available(bool forced = false)
		{
			if (Is_Open())
				return true;
			
			if (MFCC.Find(File_Name()) != null)
				return true;
			
			return base.Is_Available();
		}

		public override bool Is_Open()
		{
			if (_data != null)
				return true;
			
			return base.Is_Open();
		}

		public override void Close()
		{
			_data = null;
			_position = 0;
			base.Close();
		}

		public override bool Open(FileAccess rights = FileAccess.Read)
		{
			Close();

			if ((rights & FileAccess.Write) == FileAccess.Write || base.Is_Available())
				return base.Open(rights);
			
			byte[]? data;
			int offset;
			int size;
			var mixfile = MFCC.Offset(File_Name(), out data, out offset, out size);
			if (mixfile != null)
			{
				if (data == null)
				{
					Open(mixfile.Filename);
					Searching(false);
					Set_Name(File_Name());
					Searching(true);
					Bias(0);
					Bias(offset, size);
					Seek(0, SeekOrigin.Begin);
				}
				else
					_data = data;
			}
			else
				return base.Open(rights);
			
			return true;
		}
	}
}