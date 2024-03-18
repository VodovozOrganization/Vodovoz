namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Обновление контрагента
	/// </summary>
	public class CounterpartyUpdateDto
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// Статус обновления
		/// </summary>
		public CounterpartyUpdateStatus CounterpartyUpdateStatus { get; set; }
	}
}
