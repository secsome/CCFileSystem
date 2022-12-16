namespace CCFileSystem
{
	public abstract class FileClass
	{
		public abstract string File_Name();

		public abstract string Set_Name(string filename);

		public abstract bool Create();

		public abstract bool Delete();

		public abstract bool Is_Available(bool forced = false);

		public abstract bool Is_Open();

		public abstract bool Open(string filename, FileAccess rights = FileAccess.Read);

		public abstract bool Open(FileAccess rights = FileAccess.Read);

		public abstract byte[]? Read(long length);

		public abstract long Seek(long pos, SeekOrigin dir = SeekOrigin.Current);

		public abstract long Size();

		public abstract long Write(byte[] buffer, long size);

		public abstract void Close();

	}
}