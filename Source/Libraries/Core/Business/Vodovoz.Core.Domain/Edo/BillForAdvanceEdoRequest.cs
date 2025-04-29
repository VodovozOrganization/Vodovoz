using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку счета без отгруки на предоплату по ЭДО
	/// </summary>
	public class BillForAdvanceEdoRequest : CustomerEdoRequest
	{
		private int _orderWithoutShipmentForAdvancePaymentId;

		/// <summary>
		/// Код счета без отгрузки на предоплату
		/// </summary>
		[Display(Name = "Код счета без отгрузки на предоплату")]
		public virtual int OrderWithoutShipmentForAdvancePaymentId
		{
			get => _orderWithoutShipmentForAdvancePaymentId;
			set => SetField(ref _orderWithoutShipmentForAdvancePaymentId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
