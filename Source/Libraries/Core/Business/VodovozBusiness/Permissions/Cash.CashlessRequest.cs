namespace Vodovoz.Permissions
{
	public static partial class Cash
	{
		/// <summary>
		/// Права заявок на выдачу денежных средств по безналу
		/// </summary>
		public static partial class CashlessRequest
		{
			/// <summary>
			/// Доступно создание календаря платежей
			/// </summary>
			public static string CanCreateGiveOutSchedule => "can_create_give_out_schedule";
		}
	}
}
