namespace CCFileSystem
{
	public class CDFileClass : BufferIOFileClass
	{
		static private LinkedList<string> _searchDrives = new LinkedList<string>();

		private bool _isDisabled;

		public CDFileClass(string filename) : base(filename) 
		{
			_isDisabled = false;
			Set_Name(filename);
		}
		
		public CDFileClass() : base()
		{
			_isDisabled = false;
		}

		public override string Set_Name(string filename)
		{
			base.Set_Name(filename);
			if (_isDisabled || !Is_There_Search_Drives() || base.Is_Available())
				return File_Name();
			
			foreach (string drive in _searchDrives)
			{
				string full = Path.Combine(drive, filename);
				base.Set_Name(full);
				if (base.Is_Available())
					return File_Name();
			}

			base.Set_Name(filename);
			return File_Name();
		}

		public void Searching(bool on)
		{
			_isDisabled = !on;
		}

		static public bool Is_There_Search_Drives()
		{
			return _searchDrives.Count > 0;
		}

		static void Add_Search_Drive(string path)
		{
			_searchDrives.AddLast(path);
		}

		static void Clear_Search_Drives()
		{
			_searchDrives.Clear();
		}
	}
}