using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Информация о документе, по которому есть непринятый УПД в ЭДО
	/// </summary>
	public class TimedOutDocFlowDocumentNode
	{
		/// <summary>
		/// ЭДО аккаунт организации, от которой был отправлен документ
		/// </summary>
		public string OurEdoAccount { get; set; }

		/// <summary>
		/// Заказ
		/// </summary>
		public OrderEntity Order { get; set; }

		/// <summary>
		/// Документооборот в Такском
		/// </summary>
		public TaxcomDocflow TaxcomDocflow { get; set; }

		/// <summary>
		/// Номер УПД для напоминания о принятии
		/// </summary>
		public string UpdNum { get; set; }
	}
}
