using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка на отправку счета без отгрузки на долг по ЭДО
	/// </summary>
	public class BillWithoutShipmentForDebtEdoRequest : InformalEdoRequest
	{
		/// <summary>
		/// Тип отправляемого счета
		/// </summary>
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.BillWSForDebt;
	}
}
