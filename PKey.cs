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

			// Westwood XMP uses (UINT_MAX + 1) as its base, and it's little endian
			// We have to manually convert it to C# BigInteger before we apply RSA decryption.
			BigInteger ToBigInt(byte[] src)
			{
				BigInteger result = BigInteger.Zero;
				var basev = new BigInteger(0x100000000ul);
				for (int i = 0; i < src.Length; i += 4)
					result += new BigInteger(BitConverter.ToUInt32(src, i)) * BigInteger.Pow(basev, i / 4);
				return result;
			}
			// So do we need Westwood XMP format data
			byte[] FromBigInt(BigInteger src)
			{
				MemoryStream buf = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(buf);

				List<uint> vals = new List<uint>();
				BigInteger basev = new BigInteger(0x100000000ul);
				do
				{
					vals.Add((uint)(src % basev));
					src /= basev;
				} while (src != BigInteger.Zero);
				foreach (var v in vals)
					bw.Write(v);
				return buf.ToArray().Take(PlainBlockSize).ToArray();
			}

			MemoryStream ms = new MemoryStream();
			int idx = 0;
			while (idx < data.Length)
			{
				BigInteger temp = ToBigInt(data.Skip(idx).Take(CryptBlockSize).ToArray());
				// RSA decryption: d = c ^ e mod m
				var buffer = FromBigInt(BigInteger.ModPow(temp, _exponent, _modulus));
				ms.Write(buffer, 0, buffer.Length);
				idx += CryptBlockSize;
			}

			return ms.ToArray();
		}

		private BigInteger _modulus;
		private BigInteger _exponent;
	}
}
