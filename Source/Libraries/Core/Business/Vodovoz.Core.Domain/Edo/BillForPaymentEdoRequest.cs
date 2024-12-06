using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class BillForPaymentEdoRequest : OrderEdoRequest
	{
		private int _orderWithoutShipmentForPaymentId;

		[Display(Name = "Код счета без отгрузки на постоплату")]
		public virtual int OrderWithoutShipmentForPaymentId
		{
			get => _orderWithoutShipmentForPaymentId;
			set => SetField(ref _orderWithoutShipmentForPaymentId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
