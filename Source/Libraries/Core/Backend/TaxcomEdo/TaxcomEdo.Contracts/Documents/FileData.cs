namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Данные файла
	/// </summary>
	public abstract class FileData
	{
		protected FileData(byte[] data)
		{
			Image = data;
		}
		
		/// <summary>
		/// Имя файла
		/// </summary>
		public abstract string Name { get; }
		/// <summary>
		/// Содержимое файла
		/// </summary>
		public byte[] Image { get; }
	}
}
