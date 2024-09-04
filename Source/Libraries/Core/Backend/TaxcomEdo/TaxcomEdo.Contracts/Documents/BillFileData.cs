using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Файл со счетом
	/// </summary>
	public class BillFileData : FileData
	{
		protected BillFileData(string billNumber, DateTime billDate, byte[] data) : base(data)
		{
			BillNumber = billNumber;
			BillDate = billDate;
		}
		/// <summary>
		/// Номер счета
		/// </summary>
		public string BillNumber { get; }
		/// <summary>
		/// Дата счета
		/// </summary>
		public DateTime BillDate { get; }

		public override string Name => $"Счёт №{BillNumber} от {BillDate:d}.pdf";

		public static BillFileData Create(string billId, DateTime billDate, byte[] data) =>
			new BillFileData(billId, billDate, data);
	}
}
