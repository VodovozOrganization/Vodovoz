using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
	public class NonSerialEquipmentsForRentJournalViewModelFactory : INonSerialEquipmentsForRentJournalViewModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public NonSerialEquipmentsForRentJournalViewModelFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public NonSerialEquipmentsForRentJournalViewModel CreateNonSerialEquipmentsForRentJournalViewModel(EquipmentKind equipmentKind)
		{
			return new NonSerialEquipmentsForRentJournalViewModel(
				equipmentKind,
				_uowFactory,
				ServicesConfig.InteractiveService,
				new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())),
				null);
		}
	}
}
