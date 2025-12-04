using QS.DomainModel.Entity;
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
	public class EquipmentTransferEdoRequest : InformalEdoRequest
	{
		public override OrderDocumentType OrderDocumentType => OrderDocumentType.EquipmentTransfer;
	}
}
