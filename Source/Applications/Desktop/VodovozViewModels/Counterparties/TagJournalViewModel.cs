using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Client;
using static Vodovoz.ViewModels.Counterparties.TagJournalViewModel;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class TagJournalViewModel : EntityJournalViewModelBase<Tag, TagViewModel, TagJournalNode>
	{
		public TagJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<Tag> ItemsQuery(IUnitOfWork uow)
		{
			TagJournalNode resultAlias = null;

			return uow.Session.QueryOver<Tag>()
				.SelectList(list =>
					list.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(x => x.ColorText).WithAlias(() => resultAlias.ColorText))
				.TransformUsing(Transformers.AliasToBean(typeof(TagJournalNode)));
		}
	}
}
