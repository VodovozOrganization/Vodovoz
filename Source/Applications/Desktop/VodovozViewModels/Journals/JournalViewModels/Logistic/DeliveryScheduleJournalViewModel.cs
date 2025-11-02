using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Commands;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class DeliveryScheduleJournalViewModel : SingleEntityJournalViewModelBase<DeliverySchedule, DeliveryScheduleViewModel, DeliveryScheduleJournalNode>
	{
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private OpenViewModelCommand _openViewModelCommand;

		public DeliveryScheduleJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IRoboatsViewModelFactory roboatsViewModelFactory,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false,
			Action<DeliveryScheduleFilterViewModel> filterConfiguration = null)
		: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));

			Title = "Графики доставки";

			SetFilterConfiguration(filterConfiguration);
			Filter = FilterViewModel;
			JournalFilter = FilterViewModel;

			UpdateOnChanges(typeof(DeliverySchedule));
		}

		public void SetForRoboatsCatalogExport(OpenViewModelCommand openViewModelCommand)
		{
			_openViewModelCommand = openViewModelCommand;
			UpdateActions();
		}

		private void SetFilterConfiguration(Action<DeliveryScheduleFilterViewModel> filterConfiguration)
		{
			if(filterConfiguration == null)
			{
				return;
			}

			_filterViewModel.ConfigureWithoutFiltering(filterConfiguration);
		}

		private DeliveryScheduleFilterViewModel _filterViewModel = new DeliveryScheduleFilterViewModel();

		public DeliveryScheduleFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set
			{
				SetField(ref _filterViewModel, value);
				Filter = _filterViewModel;
			}
		}

		private void UpdateActions()
		{
			if(_openViewModelCommand != null)
			{
				NodeActionsList.Clear();
				CreateActivateAction();
				CreateAddAction();
				CreateOpenAction();
				CreateDefaultDeleteAction();

				return;
			}

			CreateNodeActions();
		}

		private void CreateActivateAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any(),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);
			RowActivatedAction = selectAction;
		}

		private void CreateAddAction()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var entityConfig = EntityConfigs.First().Value;
			var action = new JournalAction("Добавить",
				(selected) => entityConfig.PermissionResult.CanCreate,
				(selected) => true,
				(selected) =>
				{
					var docConfig = entityConfig.EntityDocumentConfigurations.First();
					var viewModel = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction() as ViewModelBase;
					if(_openViewModelCommand.CanExecute(viewModel))
					{
						_openViewModelCommand.Execute(viewModel);
					}
				},
				"Insert"
				);
			NodeActionsList.Add(action);
		}

		private void CreateOpenAction()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var entityConfig = EntityConfigs.First().Value;

			string actionName = entityConfig.PermissionResult.CanRead && !entityConfig.PermissionResult.CanUpdate ? "Открыть" : "Изменить";
			bool canOpen = entityConfig.PermissionResult.CanRead || entityConfig.PermissionResult.CanUpdate;

			var action = new JournalAction(actionName,
				(selected) => canOpen && selected.Any(),
				(selected) => true,
				(selected) =>
				{
					var selectedNode = selected.FirstOrDefault() as DeliveryScheduleJournalNode;
					var docConfig = entityConfig.EntityDocumentConfigurations.First();
					var viewModel = docConfig.GetOpenEntityDlgFunction().Invoke(selectedNode) as ViewModelBase;
					if(_openViewModelCommand.CanExecute(viewModel))
					{
						_openViewModelCommand.Execute(viewModel);
					}
				},
				"Return"
				);
			NodeActionsList.Add(action);
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliverySchedule>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliverySchedule deliveryScheduleAlias = null;
			DeliveryScheduleJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => deliveryScheduleAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => deliveryScheduleAlias.Id,
				() => deliveryScheduleAlias.Name,
				() => deliveryScheduleAlias.From,
				() => deliveryScheduleAlias.To)
			);

			if(FilterViewModel != null && FilterViewModel.IsNotArchive)
			{
				itemsQuery.Where(() => !deliveryScheduleAlias.IsArchive);
			}

			itemsQuery.SelectList(list => list
					.Select(() => deliveryScheduleAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => deliveryScheduleAlias.From).WithAlias(() => resultAlias.TimeFrom)
					.Select(() => deliveryScheduleAlias.To).WithAlias(() => resultAlias.TimeTo)
					.Select(() => deliveryScheduleAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => deliveryScheduleAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
				)
				.OrderBy(x => x.From).Asc
				.OrderBy(x => x.To).Asc
				.TransformUsing(Transformers.AliasToBean(typeof(DeliveryScheduleJournalNode)));

			return itemsQuery;
		};

		protected override Func<DeliveryScheduleViewModel> CreateDialogFunction => () =>
			new DeliveryScheduleViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _deliveryScheduleRepository, _roboatsViewModelFactory);

		protected override Func<DeliveryScheduleJournalNode, DeliveryScheduleViewModel> OpenDialogFunction => (node) =>
			new DeliveryScheduleViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _deliveryScheduleRepository, _roboatsViewModelFactory);
	}

	public class DeliveryScheduleJournalNode : JournalEntityNodeBase<DeliverySchedule>
	{
		public override string Title => DeliveryTime;
		public string Name { get; set; }
		public TimeSpan TimeFrom { get; set; }
		public TimeSpan TimeTo { get; set; }
		public virtual string DeliveryTime => $"с {TimeFrom:hh\\:mm} до {TimeTo:hh\\:mm}";
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
		public bool IsArchive { get; set; }
	}
}
