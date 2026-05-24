namespace VodovozBusiness.Nodes.SalesReport
{
	public class BottlesDataNode
	{
		/// <summary>
		/// Плановое количество бутылей на возврат (из заказов)
		/// </summary>
		public int Plan { get; set; }

		/// <summary>
		/// Фактическое количество бутылей, возвращенных из маршрутных листов
		/// </summary>
		public int FactFromRouteList { get; set; }

		/// <summary>
		/// Фактическое количество бутылей, возвращенных по документам самовывоза
		/// </summary>
		public int FactFromSelfDelivery { get; set; }

		/// <summary>
		/// Общее фактическое количество возвращенных бутылей
		/// </summary>
		public int Fact => FactFromRouteList + FactFromSelfDelivery;
	}
}
