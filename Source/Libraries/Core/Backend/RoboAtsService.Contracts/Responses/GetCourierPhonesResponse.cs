namespace RoboAtsService.Contracts.Responses
{
	/// <summary>
	/// Информация о том,, куда переводить звонок для уточнения времени доставки
	/// </summary>
	public class GetCourierPhonesResponse
	{
		/// <summary>
		/// Телефон курьера
		/// </summary>
		public string CourierPhone { get; set; }

		/// <summary>
		/// Общий телефон по вопросам доставки (Телефон ВТ)
		/// </summary>
		public string CourierDispatcher { get; set; }

		/// <summary>
		/// Время для попытки дозвона
		/// </summary>
		public int CallTimeout { get; set; }
	}
}
