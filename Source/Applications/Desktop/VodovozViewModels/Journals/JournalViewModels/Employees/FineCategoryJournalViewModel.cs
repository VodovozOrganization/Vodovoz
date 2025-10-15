using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.Nodes.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class FineCategoryJournalViewModel : EntityJournalViewModelBase<
			FineCategory,
			FineCategoryViewModel,
			FineCategoryJournalNode>
	{
		private readonly ICurrentPermissionService _currentPermissionService;
		public FineCategoryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));

			TabName = $"Журнал {typeof(FineCategory).GetClassUserFriendlyName().GenitivePlural}";

			VisibleCreateAction = false;
			VisibleEditAction = false;
			VisibleDeleteAction = false;
		}

		protected override IQueryOver<FineCategory> ItemsQuery(IUnitOfWork uow)
		{
			FineCategory fineCategoryAlias = null;
			FineCategoryJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => fineCategoryAlias);
			return query
				.SelectList(list => list
					.Select(() => fineCategoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineCategoryAlias.Name).WithAlias(() => resultAlias.FineCategoryName)
				)
				.TransformUsing(Transformers.AliasToBean<FineCategoryJournalNode>());
		}
	}
}
