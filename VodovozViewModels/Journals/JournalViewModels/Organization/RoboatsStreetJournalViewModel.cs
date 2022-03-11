using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Organizations;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Commands;
using Vodovoz.ViewModels.Dialogs.Organizations;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Organizations
{
	public class RoboatsStreetJournalViewModel : SingleEntityJournalViewModelBase<RoboatsStreet, RoboatsStreetViewModel, RoboatsStreetJournalNode>
	{
		private readonly RoboatsViewModelFactory _roboatsViewModelFactory;
		private readonly ICommonServices _commonServices;
		private OpenViewModelCommand _openViewModelCommand;

		public RoboatsStreetJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			RoboatsViewModelFactory roboatsViewModelFactory,
			ICommonServices commonServices
		) : base(unitOfWorkFactory, commonServices)
		{
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			TabName = "Улицы для Roboats";

			UpdateOnChanges(
				typeof(RoboatsStreet)
			);
		}

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
				(selected) => entityConfig.PermissionResult.CanCreate,
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
			var action = new JournalAction("Изменить",
				(selected) => entityConfig.PermissionResult.CanRead,
				(selected) => entityConfig.PermissionResult.CanRead,
				(selected) => {
					var selectedNode = selected.FirstOrDefault() as RoboatsStreetJournalNode;
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

		protected override Func<IUnitOfWork, IQueryOver<RoboatsStreet>> ItemsSourceQueryFunction => (uow) => {
			RoboatsStreet roboatsStreetAlias = null;
			RoboatsStreetJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboatsStreetAlias);

			itemsQuery.Where(
				GetSearchCriterion(
					() => roboatsStreetAlias.Name,
					() => roboatsStreetAlias.Id
				)
			);

			itemsQuery.SelectList(list => list
				.Select(() => roboatsStreetAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => roboatsStreetAlias.Name).WithAlias(() => resultAlias.Street)
				.Select(() => roboatsStreetAlias.Type).WithAlias(() => resultAlias.StreetType)
				.Select(() => roboatsStreetAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
			)
			.OrderBy(x => x.Name).Asc
			.TransformUsing(Transformers.AliasToBean<RoboatsStreetJournalNode>());

			return itemsQuery;
		};

		protected override Func<RoboatsStreetViewModel> CreateDialogFunction => () =>
			new RoboatsStreetViewModel(EntityUoWBuilder.ForCreate(), _roboatsViewModelFactory, _commonServices);

		protected override Func<RoboatsStreetJournalNode, RoboatsStreetViewModel> OpenDialogFunction => (node) =>
			new RoboatsStreetViewModel(EntityUoWBuilder.ForOpen(node.Id), _roboatsViewModelFactory, _commonServices);
	}
}
