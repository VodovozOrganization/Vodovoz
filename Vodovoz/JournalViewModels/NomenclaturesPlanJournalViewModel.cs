using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.FilterViewModels.Order;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.JournalViewModels
{
    public class NomenclaturesPlanJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclaturePlanViewModel, NomenclaturePlanJournalNode, NomenclaturePlanFilterViewModel>
    {
        public NomenclaturesPlanJournalViewModel(NomenclaturePlanFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал План продаж для КЦ";
            UpdateOnChanges(typeof(Nomenclature));
        }

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultEditAction();
        }

        protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => (uow) =>
        {
            Nomenclature nomenclatureAlias = null;
            NomenclaturePlanJournalNode resultAlias = null;

            var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias);

            if (!FilterViewModel.NomenclatureFilterViewModel.RestrictArchive)
                itemsQuery.Where(() => !nomenclatureAlias.IsArchive);

            if (!FilterViewModel.NomenclatureFilterViewModel.RestrictDilers)
                itemsQuery.Where(() => !nomenclatureAlias.IsDiler);

            if (FilterViewModel.NomenclatureFilterViewModel.RestrictCategory == NomenclatureCategory.water)
                itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == FilterViewModel.NomenclatureFilterViewModel.RestrictDisposbleTare);

            if (FilterViewModel.NomenclatureFilterViewModel.RestrictCategory.HasValue)
                itemsQuery.Where(() => nomenclatureAlias.Category == FilterViewModel.NomenclatureFilterViewModel.RestrictCategory.Value);

            if (FilterViewModel.NomenclatureFilterViewModel.SelectCategory.HasValue && FilterViewModel.NomenclatureFilterViewModel.SelectSaleCategory.HasValue
                && Nomenclature.GetCategoriesWithSaleCategory().Contains(FilterViewModel.NomenclatureFilterViewModel.SelectCategory.Value))
                itemsQuery.Where(() => nomenclatureAlias.SaleCategory == FilterViewModel.NomenclatureFilterViewModel.SelectSaleCategory);

            if (FilterViewModel.IsOnlyPlanned)
                itemsQuery.Where(n => n.PlanDay != null || n.PlanMonth != null);

            itemsQuery.Where(GetSearchCriterion(
                () => nomenclatureAlias.Name,
                () => nomenclatureAlias.Id,
                () => nomenclatureAlias.OnlineStoreExternalId)
            );

            itemsQuery.Where(() => !nomenclatureAlias.IsSerial)
                .SelectList(list => list
                    .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
                    .Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
                    .Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
                    .Select(() => nomenclatureAlias.PlanDay).WithAlias(() => resultAlias.PlanDay)
                    .Select(() => nomenclatureAlias.PlanMonth).WithAlias(() => resultAlias.PlanMonth)
                )
                .OrderBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<NomenclaturePlanJournalNode>());

            return itemsQuery;
        };

        protected override Func<NomenclaturePlanViewModel> CreateDialogFunction => () => throw new InvalidOperationException("Нельзя создавать номенклатуры из данного журнала");

        protected override Func<NomenclaturePlanJournalNode, NomenclaturePlanViewModel> OpenDialogFunction =>
            node => new NomenclaturePlanViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);

    }
}
