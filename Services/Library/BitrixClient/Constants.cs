namespace Bitrix
{
	public static class Constants
	{
		public const string ApiUrl = "https://vodovoz.bitrix24.ru";

		//Статусы сделок для разных направлений(досок) имеют свой id.
		//На данный момент тут находятся статусы для доски "заказы сайт" (C24 - по сути id направления/категории).
		//Для доски категории 0 (ИМ оплата нал/сайт) вроде этого префикса нет
		/// <summary>
		/// Id статуса сделки "Завести в ДВ"
		/// </summary>
		public const string CreateInDVDealStageId = "C24:FINAL_INVOICE";//13
		public const string InProgressDealStageId = "C24:1";//уточнить
		public const string DVErrorDealStageId = "C24:2";//уточнить
		public const string FailDealStageId = "C24:LOSE";//7
		public const string SuccessDealStageId = "C24:WON";
	}
}
