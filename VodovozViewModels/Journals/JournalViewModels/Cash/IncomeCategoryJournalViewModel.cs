using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class IncomeCategoryJournalViewModel : FilterableSingleEntityJournalViewModelBase
        <
            IncomeCategory,
            IncomeCategoryViewModel,
            IncomeCategoryJournalNode,
            IncomeCategoryJournalFilterViewModel
        >
    {
        private readonly IEmployeeJournalFactory _employeeJournalFactory;
        private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
        private readonly IIncomeCategoryJournalFactory _incomeCategoryJournalFactory;

        public IncomeCategoryJournalViewModel(
	        IncomeCategoryJournalActionsViewModel journalActionsViewModel,
            IncomeCategoryJournalFilterViewModel journalFilterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IEmployeeJournalFactory employeeJournalFactory,
            ISubdivisionJournalFactory subdivisionJournalFactory,
	        IIncomeCategoryJournalFactory incomeCategoryJournalFactory
        ) : base(journalActionsViewModel, journalFilterViewModel, unitOfWorkFactory, commonServices)
        {
            _employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
            _subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
            _incomeCategoryJournalFactory =
	            incomeCategoryJournalFactory ?? throw new ArgumentNullException(nameof(incomeCategoryJournalFactory));

            TabName = "Категории прихода";
            
            UpdateOnChanges(
                typeof(IncomeCategory),
                typeof(Subdivision)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<IncomeCategory>> ItemsSourceQueryFunction => (uow) => {
            IncomeCategoryJournalNode resultAlias = null;

            var query = uow.Session.QueryOver<IncomeCategory>();

            IncomeCategory level1Alias = null;
            IncomeCategory level2Alias = null;
            IncomeCategory level3Alias = null;
            IncomeCategory level4Alias = null;
            IncomeCategory level5Alias = null;
            Subdivision subdivisionAlias = null;
            // При цепочке связи:
            // 5 <- 4 <- 3 <- 2 <- 1
            // Уровни распределяются как
            // lvl1 - 5
            // lvl2 - 4|5
            // lvl3 - 3|4|5
            // lvl4 - 2|3|4|5
            // lvl5 - 1|2|3|4|5
            query = uow.Session.QueryOver<IncomeCategory>(() => level1Alias)
                .Left.JoinAlias(() => level1Alias.Parent, () => level2Alias)
                .Left.JoinAlias(() => level2Alias.Parent, () => level3Alias)
                .Left.JoinAlias(() => level3Alias.Parent, () => level4Alias)
                .Left.JoinAlias(() => level4Alias.Parent, () => level5Alias)
                .Left.JoinAlias(() => level1Alias.Subdivision, () => subdivisionAlias);
            
            if (!FilterViewModel.ShowArchive) 
                query.Where(x => !x.IsArchive);
            switch (FilterViewModel.Level)
            {
                case LevelsFilter.Level1:
                    query.Where(Restrictions.IsNull(Projections.Property(() => level2Alias.Id)));
                    break;
                case LevelsFilter.Level2:
                    query.Where(Restrictions.IsNull(Projections.Property(() => level3Alias.Id)));
                    break;
                case LevelsFilter.Level3:
                    query.Where(Restrictions.IsNull(Projections.Property(() => level4Alias.Id)));
                    break;
                case LevelsFilter.Level4:
                    query.Where(Restrictions.IsNull(Projections.Property(() => level5Alias.Id)));
                    break;
            }
            
            query.SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(() => level1Alias.Name).WithAlias(() => resultAlias.Level5)
                    .Select(() => level2Alias.Name).WithAlias(() => resultAlias.Level4)
                    .Select(() => level3Alias.Name).WithAlias(() => resultAlias.Level3)
                    .Select(() => level4Alias.Name).WithAlias(() => resultAlias.Level2)
                    .Select(() => level5Alias.Name).WithAlias(() => resultAlias.Level1)
                    .Select(() => FilterViewModel.Level).WithAlias(() => resultAlias.LevelFilter)
                    .Select(() => subdivisionAlias.ShortName).WithAlias(() => resultAlias.Subdivision)
                    .Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
                ).TransformUsing(Transformers.AliasToBean<IncomeCategoryJournalNode>())
                .OrderBy(x => x.Name);
            
            query.Where(
                GetSearchCriterion(
                    () => level5Alias.Name,
                    () => level4Alias.Name,
                    () => level3Alias.Name,
                    () => level2Alias.Name,
                    () => level1Alias.Name,
                    () => level5Alias.Id,
                    () => level4Alias.Id,
                    () => level3Alias.Id,
                    () => level2Alias.Id,
                    () => level1Alias.Id
                )
            );
            return query;
        };
        
        protected override Func<IncomeCategoryViewModel> CreateDialogFunction => () => new IncomeCategoryViewModel(
            EntityUoWBuilder.ForCreate(),
            UnitOfWorkFactory,
            CommonServices,
            _employeeJournalFactory,
            _subdivisionJournalFactory,
            _incomeCategoryJournalFactory
        );
        
        protected override Func<JournalEntityNodeBase, IncomeCategoryViewModel> OpenDialogFunction =>
            node => new IncomeCategoryViewModel(
                EntityUoWBuilder.ForOpen(node.Id),
                UnitOfWorkFactory, 
                CommonServices, 
                _employeeJournalFactory,
                _subdivisionJournalFactory,
                _incomeCategoryJournalFactory
        );

        protected override void CreatePopupActions()
        {
            base.CreatePopupActions();

            PopupActionsList.Add(new JournalAction(
                "Архивировать",
                x => true, 
                x => true,
                selectedItems => {
                    var selectedNodes = selectedItems.Cast<IncomeCategoryJournalNode>();
                    var selectedNode = selectedNodes.FirstOrDefault();
                    if(selectedNode != null)
                    {
                        selectedNode.IsArchive = true;
                        using (var uow = UnitOfWorkFactory.CreateForRoot<IncomeCategory>(selectedNode.Id))
                        {
                            uow.Root.SetIsArchiveRecursively(true);
                            uow.Save();                   
                            uow.Commit();
                        } 
                    }
                })
            );
        }
    }
}