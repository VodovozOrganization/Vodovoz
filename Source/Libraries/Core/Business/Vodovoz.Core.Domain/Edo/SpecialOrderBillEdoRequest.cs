using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка на отправку особого счета из заказа по ЭДО
	/// </summary>
	public class SpecialOrderBillEdoRequest : InformalEdoRequest
	{
		/// <summary>
		/// Тип отправляемого счета
		/// </summary>
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.SpecialBill;
	}
}
