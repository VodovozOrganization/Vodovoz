using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку акта приёма-передачи оборудования по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку акта приёма-передачи оборудования по ЭДО",
		NominativePlural = "заявки на отправку акта приёма-передачи оборудования по ЭДО"
	)]
	public class EquipmentTransferEdoRequest : CustomerEdoRequest
	{
		private int _equipmentTransferId;
		private OrderEntity _order;

		/// <summary>
		/// Код акта приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Код акта приёма-передачи оборудования")]
		public virtual int EquipmentTransferId
		{
			get => _equipmentTransferId;
			set => SetField(ref _equipmentTransferId, value);
		}

		/// <summary>
		/// Код заказа c актом приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.EquipmentTransfer;
	}
}
