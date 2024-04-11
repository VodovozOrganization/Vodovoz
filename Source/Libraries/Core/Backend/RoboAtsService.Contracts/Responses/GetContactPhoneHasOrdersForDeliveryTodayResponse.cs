namespace RoboAtsService.Contracts.Responses
{
	/// <summary>
	/// Ответ есть ли заказы с контактным телефоном
	/// </summary>
	public class GetContactPhoneHasOrdersForDeliveryTodayResponse
	{
		/// <summary>
		/// Статус наличия заказов с контактным номером из запроса
		/// </summary>
		public bool Status { get; set; }
	}
}
