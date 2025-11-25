using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку акта приёма-передачи оборудования по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку акта приёма-передачи оборудования по ЭДО",
		NominativePlural = "заявки на отправку акта приёма-передачи оборудования по ЭДО"
	)]
	public class EquipmentTransferEdoRequest : InformalEdoRequest
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
	}
}
