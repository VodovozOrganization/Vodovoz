using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class RoboatsJournalsFactory
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly RoboatsViewModelFactory _roboatsViewModelFactory;
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;

		public RoboatsJournalsFactory(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, RoboatsViewModelFactory roboatsViewModelFactory, INomenclatureSelectorFactory nomenclatureSelectorFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
		}

		public RoboatsStreetJournalViewModel CreateStreetJournal()
		{
			return new RoboatsStreetJournalViewModel(_unitOfWorkFactory, _roboatsViewModelFactory, _commonServices);
		}

		public RoboatsWaterTypeJournalViewModel CreateWaterJournal()
		{
			return new RoboatsWaterTypeJournalViewModel(_unitOfWorkFactory, _roboatsViewModelFactory, _nomenclatureSelectorFactory, _commonServices);
		}

		public RoboAtsCounterpartyNameJournalViewModel CreateCounterpartyNameJournal()
		{
			return new RoboAtsCounterpartyNameJournalViewModel(_unitOfWorkFactory, _roboatsViewModelFactory, _commonServices);
		}

		public RoboAtsCounterpartyPatronymicJournalViewModel CreateCounterpartyPatronymicJournal()
		{
			return new RoboAtsCounterpartyPatronymicJournalViewModel(_unitOfWorkFactory, _roboatsViewModelFactory, _commonServices);
		}

		public IEntityAutocompleteSelectorFactory CreateCounterpartyNameSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboAtsCounterpartyNameJournalViewModel>(typeof(RoboAtsCounterpartyName), () =>
			{
				return CreateCounterpartyNameJournal();
			});
		}

		public IEntityAutocompleteSelectorFactory CreateCounterpartyPatronymicSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboAtsCounterpartyPatronymicJournalViewModel>(typeof(RoboAtsCounterpartyPatronymic), () =>
			{
				return CreateCounterpartyPatronymicJournal();
			});
		}
	}
}
