using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку счета без отгруки на постоплату по ЭДО
	/// </summary>
	public class BillForPaymentEdoRequest : PrimaryEdoRequest
	{
		private int _orderWithoutShipmentForPaymentId;

		/// <summary>
		/// Код счета без отгрузки на постоплату
		/// </summary>
		[Display(Name = "Код счета без отгрузки на постоплату")]
		public virtual int OrderWithoutShipmentForPaymentId
		{
			get => _orderWithoutShipmentForPaymentId;
			set => SetField(ref _orderWithoutShipmentForPaymentId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
