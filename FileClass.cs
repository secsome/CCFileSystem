namespace CCFileSystem
{
	public abstract class FileClass
	{
		public abstract string FileName();

		public abstract string SetName(string filename);

		public abstract bool Create();

		public abstract bool Delete();

		public abstract bool IsAvailable(bool forced = false);

		public abstract bool IsOpen();

		public abstract bool Open(string filename, FileAccess rights = FileAccess.Read);

		public abstract bool Open(FileAccess rights = FileAccess.Read);

		public abstract byte[]? Read(long length);

		public abstract long Seek(long pos, SeekOrigin dir = SeekOrigin.Current);

		public abstract long Size();

		public abstract long Write(byte[] buffer, long size);

		public abstract void Close();

	}
}