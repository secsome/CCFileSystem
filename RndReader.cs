namespace CCFileSystem
{
	public class RndReader : CCReader
	{
		public static RndReader CryptRandom = new RndReader(Environment.TickCount);

		private int _seed;
		private Random _rand;
		public RndReader(int seed) : base()
		{
			_seed = seed;
			_rand = new Random(seed);
		}

		public void Reset(int seed)
		{
			_seed = seed;
			_rand = new Random(seed);
		}

		public override byte[]? Get(long length)
		{
			if (length < 1)
				return base.Get(length);
			
			byte[] ret = new byte[length];
			_rand.NextBytes(ret);
			return ret;
		}
	}
}