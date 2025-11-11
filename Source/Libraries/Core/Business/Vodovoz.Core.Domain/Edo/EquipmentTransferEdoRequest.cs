using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку акта приёма-передачи оборудования по ЭДО
	/// </summary>
	public class EquipmentTransferEdoRequest : OrderEdoRequest
	{
		private int _equipmentTransferId;

		/// <summary>
		/// Код акта приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Код акта приёма-передачи оборудования")]
		public virtual int EquipmentTransferId
		{
			get => _equipmentTransferId;
			set => SetField(ref _equipmentTransferId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.EquipmentTransfer;
	}
}
