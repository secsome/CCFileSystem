using System.Formats.Asn1;
using System.Numerics;

namespace CCFileSystem
{
	public class PKey
	{
		public PKey(bool fast = true)
		{
			if (fast)
				Set_Fast_Exponent();
		}

		private BigInteger DER_Decode(byte[] encoded)
		{
			AsnReader reader = new AsnReader(encoded, AsnEncodingRules.DER);
			return reader.ReadInteger();
		}

		public void Set_Public_Key(string pubkey)
		{
			_modulus = DER_Decode(Convert.FromBase64String(pubkey));
			_bitPrecision = _modulus.GetBitLength() - 1;
		}

		public void Set_Private_Key(string prikey)
		{
			_exponent = DER_Decode(Convert.FromBase64String(prikey));
		}

		public void Set_Fast_Exponent()
		{
			_exponent = new BigInteger(0x10001);
		}

		public byte[] Encrypt(byte[] data)
		{
			// TODO
			throw new NotImplementedException();
		}

		public byte[] Decrypt(byte[] data)
		{
			// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/BigInteger.cs
			// ToByteArray and the byte[] CTOR is just what we need
			MemoryStream ms = new MemoryStream();
			int idx = 0;
			while (idx < data.Length)
			{
				BigInteger temp = new BigInteger(data.Skip(idx).Take(Crypt_Block_Size).ToArray());
				// RSA decryption: d = c ^ e mod m
				var buffer = BigInteger.ModPow(temp, _exponent, _modulus).ToByteArray().Take(Plain_Block_Size).ToArray();
				ms.Write(buffer);
				idx += Crypt_Block_Size;
			}

			return ms.ToArray();
		}

		public int Plain_Block_Size => ((int)_bitPrecision - 1) / 8;
		public int Crypt_Block_Size => Plain_Block_Size + 1;
		public int Block_Count(int plain_length)
		{ 
			return (plain_length - 1) / Plain_Block_Size + 1; 
		}
		
		private BigInteger _modulus;
		private BigInteger _exponent;
		private long _bitPrecision;
	}
}
