using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class SubtypesJournalViewModel : EntityJournalViewModelBase<
		CounterpartySubtype,
		SubtypeViewModel,
		SubtypesJournalViewModel.Node>
	{
		public SubtypesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Журнал " + typeof(CounterpartySubtype).GetClassUserFriendlyName().GenitivePlural;
		}

		protected override IQueryOver<CounterpartySubtype> ItemsQuery(IUnitOfWork unitOfWork)
		{
			Node resultAlias = null;

			CounterpartySubtype counterpartySubtypeAlias = null;

			var queryOver = unitOfWork.Session.QueryOver(() => counterpartySubtypeAlias);

			queryOver.Where(GetSearchCriterion(
				() => counterpartySubtypeAlias.Id,
				() => counterpartySubtypeAlias.Name));

			return queryOver
				.SelectList(list =>
					list.SelectGroup(() => counterpartySubtypeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => counterpartySubtypeAlias.Name).WithAlias(() => resultAlias.Title))
				.TransformUsing(Transformers.AliasToBean<Node>());
		}
	}
}
