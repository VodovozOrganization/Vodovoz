using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Файл с актом приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferFileData : FileData
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public string OrderNumber { get; set; }
		/// <summary>
		/// Дата документа
		/// </summary>
		public DateTime DocumentDate { get; set; }

		/// <summary>
		/// Имя файла <see cref="FileData.Name"/>
		/// </summary>
		public override string Name => $"Акт приёма-передачи оборудования по заказу №{OrderNumber} от {DocumentDate:d}.pdf";
	}
}

