using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Commands;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class RoboAtsCounterpartyPatronymicJournalViewModel : SingleEntityJournalViewModelBase<RoboAtsCounterpartyPatronymic, RoboAtsCounterpartyPatronymicViewModel,
		RoboAtsCounterpartyPatronymicJournalNode>
	{
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private OpenViewModelCommand _openViewModelCommand;

		public RoboAtsCounterpartyPatronymicJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IRoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Отчества контрагентов Roboats";

			UpdateOnChanges(typeof(RoboAtsCounterpartyPatronymic));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
		}

		protected override Func<IUnitOfWork, IQueryOver<RoboAtsCounterpartyPatronymic>> ItemsSourceQueryFunction => (uow) =>
		{
			RoboAtsCounterpartyPatronymic roboAtsCounterpartyPatronymicAlias = null;
			RoboAtsCounterpartyPatronymicJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboAtsCounterpartyPatronymicAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => roboAtsCounterpartyPatronymicAlias.Id,
				() => roboAtsCounterpartyPatronymicAlias.Patronymic)
			);

			itemsQuery.SelectList(list => list
					.Select(() => roboAtsCounterpartyPatronymicAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => roboAtsCounterpartyPatronymicAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => roboAtsCounterpartyPatronymicAlias.Accent).WithAlias(() => resultAlias.Accent)
					.Select(() => roboAtsCounterpartyPatronymicAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
				)
				.OrderBy(() => roboAtsCounterpartyPatronymicAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<RoboAtsCounterpartyPatronymicJournalNode>());

			return itemsQuery;
		};

		public void SetForRoboatsCatalogExport(OpenViewModelCommand openViewModelCommand)
		{
			_openViewModelCommand = openViewModelCommand;
			UpdateActions();
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
				(selected) => {
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
				(selected) => {
					var selectedNode = selected.FirstOrDefault() as RoboAtsCounterpartyPatronymicJournalNode;
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


		protected override Func<RoboAtsCounterpartyPatronymicViewModel> CreateDialogFunction => () =>
			new RoboAtsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, _roboatsViewModelFactory, commonServices);

		protected override Func<RoboAtsCounterpartyPatronymicJournalNode, RoboAtsCounterpartyPatronymicViewModel> OpenDialogFunction =>
			(node) => new RoboAtsCounterpartyPatronymicViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, _roboatsViewModelFactory, commonServices);
	}
}
