global using MFCC = CCFileSystem.MixFile<CCFileSystem.CCFileClass>;

namespace CCFileSystem
{
	public class MixFile<TFile> where TFile : RawFileClass, new()
	{
		private static LinkedList<MixFile<TFile>> _list = new LinkedList<MixFile<TFile>>();

		public static readonly int BlowfishKeySourceLength = 80;
		public static readonly int BlowfishKeyLength = 56;
		public static readonly int BlowfishBlockSize = 8;

		private List<SubBlock> _subBlocks;
		private string _filename;
		private long _dataStart;
		private int _count;
		private int _dataSize;
		private bool _isDigest;
		private bool _isEncrypted;
		private TFile _file;
		private byte[]? _data;

		public string Filename => _filename;
		public List<SubBlock> SubBlocks => _subBlocks;

		public static MixFile<TFile>? Find(string filename)
		{
			byte[]? data;
			int offset;
			int size;
			return Offset(filename, out data, out offset, out size);
		}

		public struct SubBlock : IEquatable<SubBlock>, IComparable<SubBlock>
		{
			public bool Equals(SubBlock b)
			{
				return CRC == b.CRC;
			}

			public int CompareTo(SubBlock other)
			{
				if (CRC < other.CRC)
					return -1;
				else if (CRC > other.CRC)
					return 1;
				else
					return 0;
			}

			public static readonly int MemorySize = 12;
			public int CRC;
			public int Offset;
			public int Size;
		}

		public MixFile(string filename, PKey pkey)
		{
			_file = new TFile();
			_file.Set_Name(filename);
			if (!_file.Is_Available() || !_file.Open())
				throw new FileNotFoundException();

			FileReader freader = new FileReader(_file);
			PKReader preader = new PKReader(true, RndReader.CryptRandom);
			CCReader reader = freader;
			short First = BitConverter.ToInt16(reader.Get(2));
			short Second = BitConverter.ToInt16(reader.Get(2));
			byte[] subblocks;
			if (First == 0)
			{
				_isDigest = (Second & 0x1) != 0;
				_isEncrypted = (Second & 0x2) != 0;

				if (_isEncrypted)
				{
					// Blowfish requires 8 bytes per block
					preader.Key(pkey);
					preader.Get_From(reader);
					reader = preader;
				}
				_count = BitConverter.ToInt16(reader.Get(2));
				_dataSize = BitConverter.ToInt16(reader.Get(4));
			}
			else
			{
				_count = First;
				_file.Seek(2, SeekOrigin.Begin);
				_dataSize = BitConverter.ToInt16(reader.Get(4));
			}

			var t = reader.Get(_count * SubBlock.MemorySize);
			if (t == null)
				throw new FileLoadException();
			subblocks = t;
			_subBlocks = new List<SubBlock>();
			for (int i = 0; i < _count; ++i)
			{
				var block = new SubBlock();
				block.CRC = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i);
				block.Offset = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i + 4);
				block.Size = BitConverter.ToInt32(subblocks, SubBlock.MemorySize * i + 8);
				_subBlocks.Add(block);
			}

			_dataStart = _file.Seek(0);
			_filename = filename;
			_data = null;
		}

		~MixFile()
		{
			_data = null;
		}

		public static bool Add_Mix_File(string filename, PKey pkey)
		{
			try
			{
				_list.AddLast(new MixFile<TFile>(filename, pkey));
				return true;
			}
			catch { return false; }
		}

		public static bool Remove_Mix_File(string filename)
		{
			try
			{
				var mix = Finder(filename);
				if (mix != null)
				{
					_list.Remove(mix);
					return true;
				}
				return false;
			}
			catch { return false; }
		}


		public static MixFile<TFile>? Offset(string filename, out byte[]? data, out int offset, out int size)
		{
			data = null;
			offset = 0;
			size = 0;
			if (string.IsNullOrEmpty(filename))
				return null;

			SubBlock block = new SubBlock();
			block.CRC = Calculate_CRC(filename);

			foreach (var mix in _list)
			{
				int idx = mix.SubBlocks.BinarySearch(block);
				if (idx >= 0)
				{
					block = mix.SubBlocks[idx];
					size = block.Size;
					offset = block.Offset;
					if (mix._data != null)
						data = mix._data.Take(block.Offset).ToArray(); // TODO - Reduce memory waste here 
					if (mix._data == null)
						offset += (int)mix._dataStart;
					return mix;
				}
			}

			return null;
		}

		static private MixFile<TFile>? Finder(string filename)
		{
			foreach (var mix in _list)
			{
				string ext = Path.GetExtension(mix.Filename);
				string name = Path.GetFileName(mix.Filename);
				if (Path.Combine(name, ext).Equals(filename, StringComparison.OrdinalIgnoreCase))
					return mix;
			}

			return null;
		}

		public bool Cache(byte[]? buffer)
		{
			if (_data != null)
				return true;

			if (buffer != null)
			{
				if (buffer.Length == 0 || buffer.Length >= _dataSize)
					_data = buffer;
			}
			else
				_data = new byte[_dataSize];

			if (_data != null)
			{
				TFile file = new TFile();
				file.Set_Name(_filename);
				FileReader freader = new FileReader(file);
				CCReader reader = freader;

				SHAReader sha = new SHAReader();
				if (_isDigest)
				{
					sha.Get_From(freader);
					reader = sha;
				}

				file.Open();
				file.Bias(0);
				file.Bias(_dataStart);

				_data = reader.Get(_dataSize);
				if (_data == null || _data.Length != _dataSize)
					throw new FileLoadException();

				if (_isDigest)
				{
					var sha1 = sha.Result();
					var sha2 = freader.Get(20);
					if (sha2 == null || !sha1.SequenceEqual(sha2))
					{
						_data = null;
						return false;
					}
				}

				return true;
			}

			_data = null;
			return false;
		}

		public static bool Cache(string filename, byte[]? buffer)
		{
			MixFile<TFile>? mix = Finder(filename);
			if (mix == null)
				return false;

			return mix.Cache(buffer);
		}

		public void Free()
		{
			_data = null;
		}

		static public bool Free(string filename)
		{
			MixFile<TFile>? mix = Finder(filename);
			if (mix == null)
				return false;

			mix.Free();
			return true;
		}

		private static int Calculate_CRC(string s)
		{
			// Westwood CRCEngine consider 4 bytes as a block, so padding is required sometimes.
			s = s.ToUpper(); // Only uses upper case
			int residue = s.Length % 4;
			if (residue != 0) // Has residue data, needs padding
			{
				// Now fill others by the first byte in this block
				s += (char)residue; // First padding is residue length
				char filler = s[s.Length - residue - 1];
				for (int i = 0; i < 3 - residue; ++i)
					s += filler;
			}

			return CRC.Memory(System.Text.Encoding.UTF8.GetBytes(s));
		}

	}


}