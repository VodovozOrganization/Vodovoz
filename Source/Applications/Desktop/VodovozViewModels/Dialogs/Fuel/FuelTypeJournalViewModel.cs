using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools;
using static Vodovoz.ViewModels.Dialogs.Fuel.FuelTypeJournalViewModel;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public partial class FuelTypeJournalViewModel : EntityJournalViewModelBase<
			FuelType,
			FuelTypeViewModel,
			FuelTypeJournalNode>
	{
		private IPermissionResult _premissionResult;

		public FuelTypeJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
			: base(unitOfWorkFactory,
				  interactiveService,
				  navigationManager,
				  deleteEntityService,
				  currentPermissionService)
		{
			Title = $"Журнал {typeof(FuelType).GetClassUserFriendlyName().GenitivePlural.CapitalizeSentence()}";
		}

		protected override IQueryOver<FuelType> ItemsQuery(IUnitOfWork unitOfWork)
		{
			FuelTypeJournalNode resultAlias = null;

			return unitOfWork.Session.QueryOver<FuelType>()
				.SelectList(list =>
					list.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Title))
				.TransformUsing(Transformers.AliasToBean(typeof(FuelTypeJournalNode)));
		}

		protected override void CreateNodeActions()
		{
			_premissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(FuelType));

			NodeActionsList.Clear();

			CreateSelectAction();

			var addAction = new JournalAction("Добавить",
				(selected) => _premissionResult.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => _premissionResult.CanUpdate && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.OfType<FuelTypeJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any(),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}
			NodeActionsList.Add(selectAction);
		}

		protected override void EditEntityDialog(FuelTypeJournalNode node)
		{
			NavigationManager.OpenViewModel<FuelTypeViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(node.Id));
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<FuelTypeViewModel, IEntityUoWBuilder>(
				this,
				EntityUoWBuilder.ForCreate());
		}
	}
}
