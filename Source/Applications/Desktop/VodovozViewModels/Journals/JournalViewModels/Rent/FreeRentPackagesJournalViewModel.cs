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
    public class FreeRentPackagesJournalViewModel 
        : EntityJournalViewModelBase<FreeRentPackage, FreeRentPackageViewModel, FreeRentPackagesJournalNode>
    {
	    public FreeRentPackagesJournalViewModel(
		    IUnitOfWorkFactory unitOfWorkFactory,
		    IInteractiveService interactiveService,
		    INavigationManager navigationManager,
		    ICurrentPermissionService currentPermissionService = null,
		    IDeleteEntityService deleteEntityService = null)
		    : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
	    {
		    
	    }

        protected override IQueryOver<FreeRentPackage> ItemsQuery(IUnitOfWork uow)
        {
            FreeRentPackagesJournalNode resultAlias = null;
            EquipmentKind equipmentKindAlias = null;

            return uow.Session.QueryOver<FreeRentPackage>()
                .Left.JoinAlias(x => x.EquipmentKind, () => equipmentKindAlias)
                .Where(GetSearchCriterion<FreeRentPackage>(x => x.Name))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                    .Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.EquipmentKindName))
                .OrderBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<FreeRentPackagesJournalNode>());
        }
    }
}