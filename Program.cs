namespace CCFileSystem;
class Program
{
	static void Main(string[] args)
	{
		string gamedir = @"";

		PKey FastKey = new PKey(true);
		FastKey.SetPublicKey("AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V");
		MixFile ra2 = new MixFile(Path.Combine(gamedir, "RA2.MIX"), FastKey);
		MixFile ra2md = new MixFile(Path.Combine(gamedir, "RA2MD.MIX"), FastKey);

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
