using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
    public class NonSerialEquipmentsForRentJournalViewModelFactory : INonSerialEquipmentsForRentJournalViewModelFactory
    {
       public NonSerialEquipmentsForRentJournalViewModel CreateNonSerialEquipmentsForRentJournalViewModel(EquipmentKind equipmentKind)
        {
            return new NonSerialEquipmentsForRentJournalViewModel(
	            equipmentKind, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService, null);
        }
    }
}