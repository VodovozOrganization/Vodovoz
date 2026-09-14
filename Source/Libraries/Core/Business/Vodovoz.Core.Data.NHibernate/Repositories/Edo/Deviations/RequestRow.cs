
namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заявки ЭДО, породившей задачу
	/// </summary>
	internal class RequestRow
	{
		/// <summary>
		/// Идентификатор задачи ЭДО, созданной по заявке
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Идентификатор заявки ЭДO
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Код заказа заявки. Пустой, если заявка заведена не по заказу
		/// </summary>
		public int? OrderId { get; set; }
	}
}
