using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Organizations;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class OrganizationJournalViewModel : SingleEntityJournalViewModelBase<Organization, OrganizationDlg, OrganizationJournalNode>
	{
		public OrganizationJournalViewModel(
		IUnitOfWorkFactory unitOfWorkFactory,
		ICommonServices commonServices,
		bool hideJournalForOpenDialog = false,
		bool hideJournalForCreateDialog = false,
		INavigationManager navigation = null)
		: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog, navigation)
		{
			TabName = "Организации";
			UseSlider = true;

			UpdateOnChanges(typeof(Organization));
		}

		protected override Func<IUnitOfWork, IQueryOver<Organization>> ItemsSourceQueryFunction => (uow) =>
		{
			Organization organizationAlias = null;
			OrganizationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => organizationAlias);
			query.Where(
				GetSearchCriterion(
				() => organizationAlias.Name,
				() => organizationAlias.Id));

			var result = query.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<OrganizationJournalNode>())
				.OrderBy(x => x.Id).Asc;
			return result;
		};

		protected override Func<OrganizationDlg> CreateDialogFunction => () => new OrganizationDlg();

		protected override Func<OrganizationJournalNode, OrganizationDlg> OpenDialogFunction => (node) => new OrganizationDlg(node.Id);

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateCustomEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<OrganizationJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanUpdate || config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<OrganizationJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);

					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}
	}
}
