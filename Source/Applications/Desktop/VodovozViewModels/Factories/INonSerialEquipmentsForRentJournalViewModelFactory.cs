using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
	public interface INonSerialEquipmentsForRentJournalViewModelFactory
	{
		NonSerialEquipmentsForRentJournalViewModel CreateNonSerialEquipmentsForRentJournalViewModel(EquipmentKind equipmentKind);
	}
}