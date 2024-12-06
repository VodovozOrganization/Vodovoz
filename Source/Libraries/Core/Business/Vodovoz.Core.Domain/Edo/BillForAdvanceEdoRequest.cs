using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class BillForAdvanceEdoRequest : CustomerEdoRequest
	{
		private int _orderWithoutShipmentForAdvancePaymentId;

		[Display(Name = "Код счета без отгрузки на предоплату")]
		public virtual int OrderWithoutShipmentForAdvancePaymentId
		{
			get => _orderWithoutShipmentForAdvancePaymentId;
			set => SetField(ref _orderWithoutShipmentForAdvancePaymentId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
