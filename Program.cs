namespace CCFileSystem;

class Program
{
	static void Main(string[] args)
	{
		string gamedir = @"E:\Games\Mental Omega\Mental Omega 3.3.7";
		CCFileClass ccfile = new CCFileClass(Path.Combine(gamedir, "RA2.MIX"));
		ccfile.Is_Available(true);

		PKey FastKey = new PKey(true);
		FastKey.Set_Public_Key("AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V");
		MFCC ra2 = new MFCC(Path.Combine(gamedir, "RA2.MIX"), FastKey);
		MFCC ra2md = new MFCC(Path.Combine(gamedir, "RA2MD.MIX"), FastKey);

		Console.WriteLine("RA2.MIX info:");
		foreach (var subblock in ra2.SubBlocks)
		{
			Console.WriteLine(string.Format("{0} {1} {2}", subblock.Value.CRC, subblock.Value.Offset, subblock.Value.Size));
		}
		Console.WriteLine("RA2MD.MIX info:");
		foreach (var subblock in ra2md.SubBlocks)
		{
			Console.WriteLine(string.Format("{0} {1} {2}", subblock.Value.CRC, subblock.Value.Offset, subblock.Value.Size));
		}
	}
}
