namespace Vodovoz.EntityRepositories.Goods
{
	/// <summary>
	/// Источник запроса проверки доступности продажи номенклатуры
	/// </summary>
	public enum AvailableForSaleSourceType
	{
		/// <summary>
		/// Программа доставки воды
		/// </summary>
		WaterDelivery,
		/// <summary>
		/// Клиентское мобильное приложение
		/// </summary>
		MobileApp,
		/// <summary>
		/// Сайт ВВ
		/// </summary>
		VodovozWebsite,
		/// <summary>
		/// Сайт КС
		/// </summary>
		KulerServiceWebsite,
		/// <summary>
		/// Робот Миа
		/// </summary>
		RobotMia
	}
}
