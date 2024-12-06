using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class BillForDebtEdoRequest : OrderEdoRequest
	{
		private int _orderWithoutShipmentForDebtId;

		[Display(Name = "Код счета без отгрузки на долги")]
		public virtual int OrderWithoutShipmentForDebtId
		{
			get => _orderWithoutShipmentForDebtId;
			set => SetField(ref _orderWithoutShipmentForDebtId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
