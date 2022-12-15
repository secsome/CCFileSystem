using System.Formats.Asn1;
using System.Numerics;

namespace CCFileSystem
{
	public class PKey
	{
		public PKey(bool fast = true)
		{
			if (fast)
				SetFastExponent();
		}

		private BigInteger DERDecode(byte[] encoded)
		{
			AsnReader reader = new AsnReader(encoded, AsnEncodingRules.DER);
			return reader.ReadInteger();
		}

		public void SetPublicKey(string pubkey)
		{
			_modulus = DERDecode(Convert.FromBase64String(pubkey));
		}

		public void SetPrivateKey(string prikey)
		{
			_exponent = DERDecode(Convert.FromBase64String(prikey));
		}

		public void SetFastExponent()
		{
			_exponent = new BigInteger(0x10001);
		}

		public byte[] Decrypt(byte[] data)
		{
			const int CryptBlockSize = 40;
			const int PlainBlockSize = 39;

			// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/BigInteger.cs
			// ToByteArray and the byte[] CTOR is just what we need
			MemoryStream ms = new MemoryStream();
			int idx = 0;
			while (idx < data.Length)
			{
				BigInteger temp = new BigInteger(data.Skip(idx).Take(CryptBlockSize).ToArray());
				// RSA decryption: d = c ^ e mod m
				var buffer = BigInteger.ModPow(temp, _exponent, _modulus).ToByteArray().Take(PlainBlockSize).ToArray();
				ms.Write(buffer);
				idx += CryptBlockSize;
			}

			return ms.ToArray();
		}

		private BigInteger _modulus;
		private BigInteger _exponent;
	}
}
