using System;

namespace TaxcomEdo.Contracts.Documents
{
    /// <summary>
    /// Файл с документом заказа
    /// </summary>
    public class OrderDocumentFileData : FileData
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Дата документа
		/// </summary>
		public DateTime DocumentDate { get; set; }

		/// <summary>
		/// Имя файла <see cref="FileData.Name"/>
		/// </summary>
		public override string Name => $"Документ заказа №{OrderId} от {DocumentDate:d}.pdf";
	}
}

