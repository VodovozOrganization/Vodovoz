using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewers;

namespace Vodovoz.JournalViewModels
{
    public class WarehouseJournalViewModel : FilterableSingleEntityJournalViewModelBase<Warehouse, WarehousesView, WarehouseJournalNode, WarehouseJournalFilterViewModel>
    {
        public WarehouseJournalViewModel(WarehouseJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) 
        : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал складов";
            UpdateOnChanges(
                typeof(Warehouse)
            );
        }

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
        }

        protected override Func<IUnitOfWork, IQueryOver<Warehouse>> ItemsSourceQueryFunction => (uow) =>
        {
            Warehouse warehouseAlias = null;
            WarehouseJournalNode warehouseNodeAlias = null;
            var query = uow.Session.QueryOver<Warehouse>(() => warehouseAlias)
                .WhereRestrictionOn(w => w.Id).IsIn(FilterViewModel.Warehouses.Select(wl => wl.Id).ToList());

            query.Where(GetSearchCriterion(
                () => warehouseAlias.Id,
                () => warehouseAlias.Name
            ));
            var result = query
                .SelectList(list => list
                    .Select(w => w.Id).WithAlias(()=> warehouseNodeAlias.Id)
                    .Select(w => w.Name).WithAlias(() => warehouseNodeAlias.Name)
                )
                .OrderBy(w => w.Name).Asc
                .TransformUsing(Transformers.AliasToBean<WarehouseJournalNode>());
            return result;
        };

        protected override Func<WarehousesView> CreateDialogFunction => () => 
            throw new NotSupportedException("Не поддерживается создание склада в текущем журнале");

        protected override Func<WarehouseJournalNode, WarehousesView> OpenDialogFunction => (node) =>
            throw new NotSupportedException("Не поддерживается редактирование склада в текущем журнале");
    }
}
