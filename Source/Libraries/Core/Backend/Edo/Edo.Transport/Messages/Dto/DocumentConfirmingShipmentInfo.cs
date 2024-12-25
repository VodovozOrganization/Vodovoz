namespace Edo.Transport.Messages.Dto
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
		public string Document { get; set; } = "Универсальный передаточный документ,";
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
