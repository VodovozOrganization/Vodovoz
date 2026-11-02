using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка на отправку счета из заказа по ЭДО
	/// </summary>
	public class OrderBillEdoRequest : InformalEdoRequest
	{
		/// <summary>
		/// Тип отправляемого счета
		/// </summary>
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.Bill;
	}
}
