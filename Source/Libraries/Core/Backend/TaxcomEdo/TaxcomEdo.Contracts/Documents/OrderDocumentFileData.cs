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
		/// Номер счета; для остальных документов заказа не заполняется
		/// </summary>
		public string BillNumber { get; set; }

		/// <summary>
		/// Сохраненное имя файла счета
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Имя файла <see cref="FileData.Name"/>
		/// </summary>
		public override string Name => FileName ?? (string.IsNullOrEmpty(BillNumber)
			? $"Документ заказа №{OrderId} от {DocumentDate:d}.pdf"
			: $"Счёт №{BillNumber} от {DocumentDate:d}.pdf");
	}
}

