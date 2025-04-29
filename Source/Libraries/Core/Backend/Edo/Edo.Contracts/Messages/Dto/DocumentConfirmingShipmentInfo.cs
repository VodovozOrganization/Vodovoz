namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Документ подтверждающий отгрузку(ДокПодтвОтгр)
	/// </summary>
	public class DocumentConfirmingShipmentInfo
	{
		/// <summary>
		/// Наименование документа
		/// Обязательно для заполнения
		/// </summary>
		public string Document { get; set; } = "Договор";
		/// <summary>
		/// Номер
		/// Обязателен для заполнения
		/// </summary>
		public string Number { get; set; }
		/// <summary>
		/// Дата в формате ДД.ММ.ГГГГ
		/// </summary>
		public string Date { get; set; }
	}
}
