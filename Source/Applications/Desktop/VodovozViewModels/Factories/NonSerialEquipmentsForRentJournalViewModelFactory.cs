using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
	public class NonSerialEquipmentsForRentJournalViewModelFactory : INonSerialEquipmentsForRentJournalViewModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly INomenclatureRepository _nomenclatureRepository;

		public NonSerialEquipmentsForRentJournalViewModelFactory(
			IUnitOfWorkFactory uowFactory, 
			IInteractiveService interactiveService,
			INomenclatureRepository nomenclatureRepository
			)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new System.ArgumentNullException(nameof(nomenclatureRepository));
		}

		public NonSerialEquipmentsForRentJournalViewModel CreateNonSerialEquipmentsForRentJournalViewModel(EquipmentKind equipmentKind)
		{
			return new NonSerialEquipmentsForRentJournalViewModel(
				equipmentKind,
				_uowFactory,
				_interactiveService,
				_nomenclatureRepository,
				null);
		}
	}
}
