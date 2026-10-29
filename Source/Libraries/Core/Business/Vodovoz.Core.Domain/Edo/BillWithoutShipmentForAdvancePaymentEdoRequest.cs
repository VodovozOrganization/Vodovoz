using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка на отправку счета без отгрузки на предоплату по ЭДО
	/// </summary>
	public class BillWithoutShipmentForAdvancePaymentEdoRequest : InformalEdoRequest
	{
		/// <summary>
		/// Тип отправляемого счета
		/// </summary>
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.BillWSForAdvancePayment;
	}
}
