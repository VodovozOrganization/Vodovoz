using System.Text.Json.Serialization;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Данные файла
	/// </summary>
	[JsonDerivedType(typeof(BillFileData))]
	[JsonDerivedType(typeof(OrderDocumentFileData))]
	public abstract class FileData
	{
		/// <summary>
		/// Имя файла
		/// </summary>
		public abstract string Name { get; }
		/// <summary>
		/// Содержимое файла
		/// </summary>
		public byte[] Image { get; set; }
	}
}
