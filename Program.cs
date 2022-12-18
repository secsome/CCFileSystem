namespace CCFileSystem;

class Program
{
	static void Init_Boostrap_Mixfiles(PKey key)
	{
		for (int i = 99; i >= 0; --i)
			MFCC.Add_Mix_File(string.Format("EXPANDMO{0:02d}.MIX", i), key);

		MFCC.Add_Mix_File("RA2MD.MIX", key);
		MFCC.Add_Mix_File("RA2.MIX", key);
		MFCC.Add_Mix_File("CACHEMD.MIX", key);
		MFCC.Add_Mix_File("CACHE.MIX", key);
		MFCC.Add_Mix_File("LOCALMD.MIX", key);
		MFCC.Add_Mix_File("LOCAL.MIX", key);
	}

	static void Main(string[] args)
	{
		const string gamedir = @"";

		if (!string.IsNullOrWhiteSpace(gamedir))
			Environment.CurrentDirectory = gamedir;

		PKey FastKey = new PKey(true);
		FastKey.Set_Public_Key("AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V");

		Init_Boostrap_Mixfiles(FastKey);

		CCBuffer? data;
		int size;
		int offset;
		var mix = MFCC.Offset("12metfnt.fnt", out data, out offset, out size);
		Console.WriteLine(string.Format("{0} {1}", offset, size));
	}
}
