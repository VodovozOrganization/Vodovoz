using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Файл со счетом
	/// </summary>
	public class BillFileData : FileData
	{
		/// <summary>
		/// Номер счета
		/// </summary>
		public string BillNumber { get; set; }
		/// <summary>
		/// Дата счета
		/// </summary>
		public DateTime BillDate { get; set; }

		/// <summary>
		/// Имя файла <see cref="FileData.Name"/>
		/// </summary>
		public override string Name => $"Счёт №{BillNumber} от {BillDate:d}.pdf";
	}
}
