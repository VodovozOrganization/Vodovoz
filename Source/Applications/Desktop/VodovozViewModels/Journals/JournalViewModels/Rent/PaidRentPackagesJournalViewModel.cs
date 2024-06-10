using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.ViewModels.ViewModels.Rent;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class PaidRentPackagesJournalViewModel
		: EntityJournalViewModelBase<PaidRentPackage, PaidRentPackageViewModel, PaidRentPackagesJournalNode>
	{
		public PaidRentPackagesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService = null,
			IDeleteEntityService deleteEntityService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			
		}

		protected override IQueryOver<PaidRentPackage> ItemsQuery(IUnitOfWork uow)
		{
			PaidRentPackagesJournalNode resultAlias = null;
			EquipmentKind equipmentKindAlias = null;

			return uow.Session.QueryOver<PaidRentPackage>()
				.Left.JoinAlias(x => x.EquipmentKind, () => equipmentKindAlias)
				.Where(GetSearchCriterion<PaidRentPackage>(
					x => x.Name, x => x.PriceDaily, x => x.PriceMonthly))
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.PriceDaily).WithAlias(() => resultAlias.PriceDaily)
					.Select(x => x.PriceMonthly).WithAlias(() => resultAlias.PriceMonthly)
					.Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.EquipmentKindName))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<PaidRentPackagesJournalNode>());
		}
	}
}