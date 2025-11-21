using System;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class NonSerialEquipmentsForRentJournalViewModel : JournalViewModelBase
	{
		private readonly EquipmentKind _equipmentKind;
		private readonly INomenclatureRepository _nomenclatureRepository;

		public NonSerialEquipmentsForRentJournalViewModel(
			EquipmentKind equipmentKind,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INomenclatureRepository nomenclatureRepository,
			INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_equipmentKind = equipmentKind;
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			
			TabName = "Оборудование для аренды";
			
			var dataLoader = new ThreadDataLoader<NomenclatureForRentNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			SelectionMode = JournalSelectionMode.Single;
			CreateNodeActions();
		}

		private IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			var query = _nomenclatureRepository.QueryAvailableNonSerialEquipmentForRent(_equipmentKind)
				.GetExecutableQueryOver(uow.Session);
			
			return query;
		}
	}
}