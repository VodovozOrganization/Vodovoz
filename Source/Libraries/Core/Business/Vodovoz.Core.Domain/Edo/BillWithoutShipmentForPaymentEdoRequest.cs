using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка на отправку счета без отгрузки на постоплату по ЭДО
	/// </summary>
	public class BillWithoutShipmentForPaymentEdoRequest : InformalEdoRequest
	{
		/// <summary>
		/// Тип отправляемого счета
		/// </summary>
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.BillWSForPayment;
	}
}
