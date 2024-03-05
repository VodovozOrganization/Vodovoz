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
	public class RoboAtsCounterpartyNameJournalViewModel : SingleEntityJournalViewModelBase<RoboAtsCounterpartyName, RoboAtsCounterpartyNameViewModel,
		RoboAtsCounterpartyNameJournalNode>
	{
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private OpenViewModelCommand _openViewModelCommand;

		public RoboAtsCounterpartyNameJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IRoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Имена контрагентов Roboats";

			UpdateOnChanges(typeof(RoboAtsCounterpartyName));
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
		}

		protected override Func<IUnitOfWork, IQueryOver<RoboAtsCounterpartyName>> ItemsSourceQueryFunction => (uow) =>
		{
			RoboAtsCounterpartyName roboAtsCounterpartyNameAlias = null;
			RoboAtsCounterpartyNameJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboAtsCounterpartyNameAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => roboAtsCounterpartyNameAlias.Id,
				() => roboAtsCounterpartyNameAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => roboAtsCounterpartyNameAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => roboAtsCounterpartyNameAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => roboAtsCounterpartyNameAlias.Accent).WithAlias(() => resultAlias.Accent)
					.Select(() => roboAtsCounterpartyNameAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
				)
				.OrderBy(() => roboAtsCounterpartyNameAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<RoboAtsCounterpartyNameJournalNode>());

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
					var selectedNode = selected.FirstOrDefault() as RoboAtsCounterpartyNameJournalNode;
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

		protected override Func<RoboAtsCounterpartyNameViewModel> CreateDialogFunction => () =>
			new RoboAtsCounterpartyNameViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, _roboatsViewModelFactory, commonServices);

		protected override Func<RoboAtsCounterpartyNameJournalNode, RoboAtsCounterpartyNameViewModel> OpenDialogFunction =>
			(node) => new RoboAtsCounterpartyNameViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, _roboatsViewModelFactory, commonServices);
	}
}
