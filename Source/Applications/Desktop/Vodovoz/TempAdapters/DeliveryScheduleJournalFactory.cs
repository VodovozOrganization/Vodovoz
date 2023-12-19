using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryScheduleJournalFactory : IDeliveryScheduleJournalFactory
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;

		public DeliveryScheduleJournalFactory(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IRoboatsViewModelFactory roboatsViewModelFactory
		)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
		}

		public bool RestrictIsNotArchive { get; set; }

		public Type EntityType => typeof(DeliverySchedule);

		public DeliveryScheduleJournalViewModel CreateJournal(JournalSelectionMode selectionMode)
		{
			var filter = new DeliveryScheduleFilterViewModel();
			if(RestrictIsNotArchive)
			{
				filter.RestrictIsNotArchive = true;
			}

			var journal = new DeliveryScheduleJournalViewModel(_unitOfWorkFactory, _commonServices, _deliveryScheduleRepository, _roboatsViewModelFactory);
			journal.FilterViewModel = filter;
			journal.SelectionMode = selectionMode;
			return journal;
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateJournal(multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single);
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			return CreateJournal(multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single);
		}

		public void Dispose()
		{
			
		}
	}
}
