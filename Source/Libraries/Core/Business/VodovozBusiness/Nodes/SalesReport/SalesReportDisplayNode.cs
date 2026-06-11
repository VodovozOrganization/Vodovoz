namespace VodovozBusiness.Nodes.SalesReport
{
	/// <summary>
	/// Нода отчета по продажам для отображения в TreeView
	/// </summary>
	public class SalesReportDisplayNode
    {
		/// <summary>
		/// Уровень группировки
		/// </summary>
        public int Level { get; set; }

		/// <summary>
		/// Идентификатор объекта
		/// </summary>
        public string Code { get; set; }

		/// <summary>
		/// Клиент
		/// </summary>
        public string Counterparty { get; set; }

		/// <summary>
		/// Точка доставки
		/// </summary>
        public string DeliveryPoint { get; set; }

		/// <summary>
		/// Данные заказа в формате Заказ/Дата/Автор
		/// </summary>
        public string OrderDetails { get; set; }

		/// <summary>
		/// Телефоны
		/// </summary>
        public string Phones { get; set; }

		/// <summary>
		/// Номенклатура
		/// </summary>
        public string Nomenclature { get; set; }

		/// <summary>
		/// Количество 
		/// </summary>
        public decimal Count { get; set; }

		/// <summary>
		/// Сумма
		/// </summary>
        public decimal Sum { get; set; }

		/// <summary>
		/// Является ли узел сводным
		/// </summary>
		public bool IsSummaryInfo { get; set; }
	}
}
