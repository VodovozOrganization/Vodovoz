using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
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
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public DistrictJournalViewModel(DistrictJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(
            filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал районов";
            
            EnableAddButton = false;
            EnableDeleteButton = false;
            EnableEditButton = false;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

        public bool EnableAddButton { get; set; }
        public bool EnableDeleteButton { get; set; }
        public bool EnableEditButton { get; set; }

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
        protected override Func<DistrictViewModel> CreateDialogFunction => () => 
            new DistrictViewModel(EntityUoWBuilder.ForCreate(), _unitOfWorkFactory, commonServices);

        protected override Func<DistrictJournalNode, DistrictViewModel> OpenDialogFunction => node => 
            new DistrictViewModel(EntityUoWBuilder.ForOpen(node.Id), _unitOfWorkFactory, commonServices);

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
