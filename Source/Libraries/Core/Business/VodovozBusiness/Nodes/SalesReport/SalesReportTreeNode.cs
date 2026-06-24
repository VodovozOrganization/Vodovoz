using System.Collections.Generic;

namespace VodovozBusiness.Nodes.SalesReport
{
	/// <summary>
	/// Нода отчета по продажам
	/// </summary>
	public class SalesReportTreeNode
	{
		/// <summary>
		/// Заголовок ноды
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Ноды отчета
		/// </summary>
		public SalesReportDataNode Data { get; set; }

		/// <summary>
		/// Дочерние ноды после группировки
		/// </summary>
		public IList<SalesReportTreeNode> Children { get; set; } = new List<SalesReportTreeNode>();

		/// <summary>
		/// Общее количество
		/// </summary>
		public decimal TotalCount { get; set; }

		/// <summary>
		/// Общая сумма
		/// </summary>
		public decimal TotalSum { get; set; }

		/// <summary>
		/// Уровень группировки
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		/// Итоговая нода
		/// </summary>
		public bool IsTotalNode { get; set; }
	}
}
