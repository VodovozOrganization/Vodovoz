using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	internal class OrganizationJournalViewModel : SingleEntityJournalViewModelBase<Organization, OrganizationDlg, OrganizationJournalNode>
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
			UseSlider = false;

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
	}
}
