namespace VodovozBusiness.Nodes.SalesReport
{
	public class BottlesDataNode
	{
		/// <summary>
		/// Плановое количество бутылей на возврат (из заказов)
		/// </summary>
		public int Plan { get; set; }

		/// <summary>
		/// Общее фактическое количество возвращенных бутылей
		/// </summary>
		public int Fact { get; set; }
	}
}
