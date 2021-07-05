using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Journals.JournalViewModels
{
    public sealed class DistrictJournalViewModel: FilterableSingleEntityJournalViewModelBase<District, DistrictViewModel, DistrictJournalNode, DistrictJournalFilterViewModel>
    {
        public DistrictJournalViewModel(
	        EntitiesJournalActionsViewModel journalActionsViewModel,
	        DistrictJournalFilterViewModel filterViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        ICommonServices commonServices) 
	        : base(journalActionsViewModel, filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал районов";
        }

        protected override Func<IUnitOfWork, IQueryOver<District>> ItemsSourceQueryFunction => uow => {
            DistrictJournalNode districtJournalNode = null;
            District districtAlias = null;
            DistrictsSet districtsSetAlias = null;
            WageDistrict wageDistrictAlias = null;

            var query = uow.Session.QueryOver<District>(() => districtAlias)
                .Inner.JoinAlias(() => districtAlias.WageDistrict, () => wageDistrictAlias)
                .Inner.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias);

            if(FilterViewModel != null) {
                if(FilterViewModel.Status.HasValue)
                    query.Where(() => districtsSetAlias.Status == FilterViewModel.Status.Value);
                if(FilterViewModel.OnlyWithBorders)
                    query.Where(() => districtAlias.DistrictBorder != null);
            }

            query.Where(GetSearchCriterion(
                () => districtAlias.Id,
                () => districtAlias.DistrictName,
                () => wageDistrictAlias.Name
            ));

            var result = query
                .SelectList(list => list
                    .Select(c => c.Id).WithAlias(() => districtJournalNode.Id)
                    .Select(c => c.DistrictName).WithAlias(() => districtJournalNode.Name)
                    .Select(() => wageDistrictAlias.Name).WithAlias(() => districtJournalNode.WageDistrict)
                    .Select(() => districtsSetAlias.Status).WithAlias(() => districtJournalNode.DistrictsSetStatus)
                    .Select(() => districtsSetAlias.Id).WithAlias(() => districtJournalNode.DistrictsSetId))
                .OrderBy(() => districtsSetAlias.Id).Desc
                .TransformUsing(Transformers.AliasToBean<DistrictJournalNode>());

            return result;
        };

        protected override void InitializeJournalActionsViewModel()
        {
	        EntitiesJournalActionsViewModel.Initialize(SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected,
		        true, false, false, false);
        }

        protected override Func<DistrictViewModel> CreateDialogFunction => () => 
            new DistrictViewModel(
	            EntityUoWBuilder.ForCreate(),
	            UnitOfWorkFactory,
	            CommonServices);

        protected override Func<JournalEntityNodeBase, DistrictViewModel> OpenDialogFunction => node => 
            new DistrictViewModel(
	            EntityUoWBuilder.ForOpen(node.Id),
	            UnitOfWorkFactory,
	            CommonServices);
    }
}