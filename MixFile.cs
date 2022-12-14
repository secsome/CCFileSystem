namespace CCFileSystem
{
	public class MixFile
	{
		private static LinkedList<MixFile> _list = new LinkedList<MixFile>();

		public static readonly int BlowfishKeySourceLength = 80;
		public static readonly int BlowfishKeyLength = 56;
		public static readonly int BlowfishBlockSize = 8;

		public static MemoryStream? Retrieve(string filename)
		{
			return Retrieve(GetFileCRC(filename));
		}

		public static MemoryStream? Retrieve(int crc)
		{
			foreach (MixFile file in _list)
			{
				if (file._subBlocks.ContainsKey(crc))
				{
					var block = file._subBlocks[crc];
					file._fs.Seek(file._dataStart + block.Offset, SeekOrigin.Begin);
					return new MemoryStream(file._br.ReadBytes(block.Size), false);
				}
			}
			return null;
		}

		public struct SubBlock
		{
			public static readonly int MemorySize = 12;
			public int CRC;
			public int Offset;
			public int Size;
		}

		public MixFile(string filename, PKey pkey)
		{
			_fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			_br = new BinaryReader(_fs);

			short First = _br.ReadInt16();
			short Second = _br.ReadInt16();
			byte[] subblocks;
			if (First == 0)
			{
				_isDigest = (Second & 0x1) != 0;
				_isEncrypted = (Second & 0x2) != 0;

				if (_isEncrypted)
				{
					// Blowfish requires 8 bytes per block
					var blowfishKey = pkey.Decrypt(_br.ReadBytes(BlowfishKeySourceLength)).Take(BlowfishKeyLength).ToArray();
					var bf = new Encryption.Blowfish.BlowfishEcb(blowfishKey);
					var header = _br.ReadBytes(BlowfishBlockSize);
					bf.Decrypt(header);
					_count = BitConverter.ToInt16(header, 0);
					_dataSize = BitConverter.ToInt32(header, 2);
					// -2 because we have decoded those two bytes when decoding the header
					// (size + 7) & (!7) for align by 8 bytes
					int resLen = ((_count * SubBlock.MemorySize - 2) + 7) & (~7);
					var data = _br.ReadBytes(resLen);
					bf.Decrypt(data);
					subblocks = header.Skip(6).Concat(data).ToArray();
				}
				else
				{
					_count = _br.ReadInt16();
					_dataSize = _br.ReadInt32();
					subblocks = _br.ReadBytes(_count * SubBlock.MemorySize);
				}
			}
			else
			{
				_count = First;
				_br.BaseStream.Seek(2, SeekOrigin.Begin);
				_dataSize = _br.ReadInt32();
				subblocks = _br.ReadBytes(_count * SubBlock.MemorySize);
			}


			_subBlocks = new SortedList<int, SubBlock>();
			for (int i = 0; i < _count; ++i)
			{
				var block = new SubBlock();
				block.CRC = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i);
				block.Offset = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i + 4);
				block.Size = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i + 8);
				_subBlocks[block.CRC] = block;
			}
			_dataStart = _fs.Position;

			_list.AddFirst(this);
		}

		~MixFile()
		{
			_list.Remove(this);
		}

		private static int GetFileCRC(string s)
		{
			// Westwood CRCEngine consider 4 bytes as a block, so padding is required sometimes.
			s = s.ToUpper(); // Only uses upper case
			int residue = s.Length % 4;
			if (residue != 0) // Has residue data, needs padding
			{
				s += (char)residue; // First padding is residue length
				// Now fill others by the first byte in this block
				char filler = s[s.Length - residue];
				for (int i = 0; i < residue; ++i)
					s += filler;
			}

			return CRC.Memory(System.Text.Encoding.UTF8.GetBytes(s));
		}

		public byte[]? Digest
		{
			get
			{
				// If this file is digested, then the last 20 bytes is SHA1 sum of it.
				if (_isDigest)
				{
					_fs.Seek(-20, SeekOrigin.End);
					return _br.ReadBytes(20);
				}
				else
					return null;
			}
		}

		public long DataStart
		{
			get { return _dataStart; }
			private set { _dataStart = value; }
		}

		public int Count
		{
			get { return _count; }
			private set { _count = value; }
		}

		public int DataSize
		{
			get { return _dataSize; }
			private set { DataSize = value; }
		}

		public SortedList<int, SubBlock> SubBlocks
		{
			get { return _subBlocks; }
			private set { _subBlocks = value; }
		}

		public bool IsDigest
		{
			get { return _isDigest; }
			private set { _isDigest = value; }
		}

		public bool IsEncrypted
		{
			get { return _isEncrypted; }
			private set { _isEncrypted = value; }
		}

		private SortedList<int, SubBlock> _subBlocks;
		private long _dataStart;
		private int _count;
		private int _dataSize;
		private bool _isDigest;
		private bool _isEncrypted;
		private FileStream _fs;
		private BinaryReader _br;
	}
	

}