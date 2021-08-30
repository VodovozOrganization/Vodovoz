using System;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Journals.JournalViewModels
{
    public sealed class SectorJournalViewModel: FilterableSingleEntityJournalViewModelBase<Sector, DistrictViewModel, SectorJournalNode, SectorJournalFilterViewModel>
    {
        public SectorJournalViewModel(SectorJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(
            filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал районов";
            
            EnableAddButton = false;
            EnableDeleteButton = false;
            EnableEditButton = false;
        }

        public bool EnableAddButton { get; set; }
        public bool EnableDeleteButton { get; set; }
        public bool EnableEditButton { get; set; }

        protected override Func<IUnitOfWork, IQueryOver<Sector>> ItemsSourceQueryFunction => uow => {
            SectorJournalNode _sectorJournalNode = null;
            Sector sectorAlias = null;
            SectorVersion sectorVersion = null;
            WageSector wageSectorAlias = null;

            var query = uow.Session.QueryOver(() => sectorAlias)
	            .JoinEntityAlias(() => sectorVersion,
		            () => sectorAlias.Id == sectorVersion.Sector.Id && sectorVersion.Status == SectorsSetStatus.Active, JoinType.InnerJoin)
	            .Inner.JoinAlias(() => sectorVersion.WageSector, () => wageSectorAlias);

            if(FilterViewModel != null) {
                if(FilterViewModel.Status.HasValue)
                    query.Where(() => sectorVersion.Status == FilterViewModel.Status.Value);
                if(FilterViewModel.OnlyWithBorders)
                    query.Where(() => sectorVersion.Polygon != null);
            }

            query.Where(GetSearchCriterion(
                () => sectorAlias.Id,
                () => sectorVersion.SectorName,
                () => wageSectorAlias.Name
            ));

            var result = query
                .SelectList(list => list
                    .Select(c => c.Id).WithAlias(() => _sectorJournalNode.Id)
                    .Select(() => sectorVersion.SectorName).WithAlias(() => _sectorJournalNode.Name)
                    .Select(() => wageSectorAlias.Name).WithAlias(() => _sectorJournalNode.WageDistrict)
                    .Select(() => sectorVersion.Status).WithAlias(() => _sectorJournalNode.SectorsSetStatus)
                    .Select(() => sectorVersion.Id).WithAlias(() => _sectorJournalNode.SectorVersionId))
                .OrderBy(() => sectorVersion.Id).Desc
                .TransformUsing(Transformers.AliasToBean<SectorJournalNode>());

            return result;
        };
        protected override Func<DistrictViewModel> CreateDialogFunction => () => 
            new DistrictViewModel(EntityUoWBuilder.ForCreate(), QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory, commonServices);

        protected override Func<SectorJournalNode, DistrictViewModel> OpenDialogFunction => node => 
            new DistrictViewModel(EntityUoWBuilder.ForOpen(node.Id), QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory, commonServices);

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
            
            if(EnableAddButton)
                CreateDefaultAddActions();
            if(EnableEditButton)
                CreateDefaultEditAction();
            if(EnableDeleteButton)
                CreateDefaultDeleteAction();
        }
    }
}