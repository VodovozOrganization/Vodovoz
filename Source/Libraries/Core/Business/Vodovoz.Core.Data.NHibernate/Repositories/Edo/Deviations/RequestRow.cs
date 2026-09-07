
namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заявки ЭДО, породившей задачу.
	/// Заказ читается отдельным запросом, поэтому здесь лежит только его код
	/// </summary>
	internal class RequestRow
	{
		/// <summary>
		/// Код задачи ЭДО, созданной по заявке
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Код заявки ЭДО
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Код заказа заявки. Пустой, если заявка заведена не по заказу
		/// </summary>
		public int? OrderId { get; set; }
	}
}
