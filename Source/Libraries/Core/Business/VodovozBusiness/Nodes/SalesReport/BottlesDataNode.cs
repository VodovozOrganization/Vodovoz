namespace VodovozBusiness.Nodes.SalesReport
{
	public class BottlesDataNode
	{
		/// <summary>
		/// Плановое количество бутылей на возврат (из заказов)
		/// </summary>
		public decimal Plan { get; set; }

		/// <summary>
		/// Фактическое количество бутылей, возвращенных из маршрутных листов
		/// </summary>
		public decimal FactFromRouteList { get; set; }

		/// <summary>
		/// Фактическое количество бутылей, возвращенных по документам самовывоза
		/// </summary>
		public decimal FactFromSelfDelivery { get; set; }

		/// <summary>
		/// Общее фактическое количество возвращенных бутылей
		/// </summary>
		public decimal Fact => FactFromRouteList + FactFromSelfDelivery;
	}
}
