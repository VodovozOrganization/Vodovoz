using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.ViewModels.Cash
{
	public class FinancialResponsibilityCenterJournalViewModel : EntityJournalViewModelBase<FinancialResponsibilityCenter, FinancialResponsibilityCenterViewModel, FinancialResponsibilityCenterNode>
	{
		public FinancialResponsibilityCenterJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<FinancialResponsibilityCenter> ItemsQuery(IUnitOfWork uow)
		{
			FinancialResponsibilityCenterNode resultAlias = null;

			return uow.Session.QueryOver<FinancialResponsibilityCenter>()
				.Where(GetSearchCriterion<FinancialResponsibilityCenter>(frc => frc.Name))
				.SelectList(list =>
					list.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean(typeof(FinancialResponsibilityCenterNode)));
		}
	}
}
