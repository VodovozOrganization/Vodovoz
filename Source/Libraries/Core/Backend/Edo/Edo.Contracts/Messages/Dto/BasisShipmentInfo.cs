namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Основание отгрузки(ОснованиеТип)
	/// </summary>
	public class BasisShipmentInfo
	{
		/// <summary>
		/// Наименование документа
		/// Обязательно для заполнения
		/// При отсутствии документа указывается <c>"Без документа-основания"</c>
		/// </summary>
		public string Document { get; set; } = "Без документа-основания";
		/// <summary>
		/// Номер
		/// Обязателен при заполненном наименовании, отличном от дефолтного
		/// </summary>
		public string Number { get; set; }
		/// <summary>
		/// Дата в формате ДД.ММ.ГГГГ
		/// Обязательна при заполненном наименовании, отличном от дефолтного
		/// </summary>
		public string Date { get; set; }
	}
}
