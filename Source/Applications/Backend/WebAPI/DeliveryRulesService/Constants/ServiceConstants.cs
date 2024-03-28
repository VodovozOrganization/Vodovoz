namespace DeliveryRulesService.Constants
{
	public static class ServiceConstants
	{
		public const string ErrorGetDistrictByCoordinates = "Ошибка при подборе района по координатам";

		public const string GetDistrictFromCache = "Подбор района из кэша...";

		public const string InternalErrorFromGetDeliveryRule = "Возникла внутренняя ошибка при получении правила доставки";

		public const string CheckingFastDeliveryAvailable = "Проверка доступности доставки за час";
		
		public static string DistrictNotFoundByCoordinates =>
			"Невозможно получить информацию о правилах доставки, т.к. по координатам {Latitude}, {Longitude} не был найден район";

		public static string DistrictNotFoundByCoordinatesStringFormat =>
			"Невозможно получить информацию о правилах доставки, т.к. по координатам {0}, {1} не был найден район";

		public static string RequestToGetDeliveryRules(bool extended = false)
		{
			var str = extended ? "расширенных " : string.Empty;
			return "Поступил запрос на получение " + str + "правил доставки";
		}
	}
}
