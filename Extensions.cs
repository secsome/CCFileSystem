namespace CCFileSystem
{
	static class ArrayExtension
	{
		public static void MemCopy<T>(this T[] src, long srcoff, T[] dst, long dstoff, long length) where T : unmanaged
		{
			unsafe
			{
				IntPtr psrc = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(src, 0);
				IntPtr pdst = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(dst, 0);
				Buffer.MemoryCopy(
					((IntPtr)(psrc.ToInt64() + srcoff)).ToPointer(),
					((IntPtr)(pdst.ToInt64() + dstoff)).ToPointer(),
					length * sizeof(T),
					length * sizeof(T)
				);
			}
		}

		public static T[] LongTake<T>(this T[] src, long len) where T : unmanaged
		{
			T[] tmp = new T[len];
			src.MemCopy(0, tmp, 0, len);
			return tmp;
		}
	}
}