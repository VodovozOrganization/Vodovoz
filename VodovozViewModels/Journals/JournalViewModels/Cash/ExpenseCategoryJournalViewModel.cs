using System;
using System.IO;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
    public class ExpenseCategoryJournalViewModel: FilterableSingleEntityJournalViewModelBase
        <
            ExpenseCategory,
            ExpenseCategoryViewModel,
            ExpenseCategoryJournalNode, 
            ExpenseCategoryJournalFilterViewModel
        >
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IFileChooserProvider fileChooserProvider;

        public ExpenseCategoryJournalViewModel(
            ExpenseCategoryJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IFileChooserProvider fileChooserProvider
            ) : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.fileChooserProvider =
                fileChooserProvider ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            
            
            TabName = "Категории расхода";
            
            UpdateOnChanges(
                typeof(ExpenseCategory),
                typeof(Subdivision)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<ExpenseCategory>> ItemsSourceQueryFunction => (uow) => {
            ExpenseCategoryJournalNode resultAlias = null;

            var query = uow.Session.QueryOver<ExpenseCategory>();

            query.Where(
                (ICriterion) GetSearchCriterion<ExpenseCategory>(
                    x => x.Id,
                    x => x.Name
                )
            );

            ExpenseCategory level1Alias = null;
            ExpenseCategory level2Alias = null;
            ExpenseCategory level3Alias = null;
            ExpenseCategory level4Alias = null;
            ExpenseCategory level5Alias = null;
            Subdivision subdivisionAlias = null;

            var result = uow.Session.QueryOver<ExpenseCategory>(() => level1Alias)
                .Left.JoinAlias(() => level1Alias.Parent, () => level2Alias)
                .Left.JoinAlias(() => level2Alias.Parent, () => level3Alias)
                .Left.JoinAlias(() => level3Alias.Parent, () => level4Alias)
                .Left.JoinAlias(() => level4Alias.Parent, () => level5Alias)
                .Left.JoinAlias(() => level1Alias.Subdivision, () => subdivisionAlias);
            
            if (!FilterViewModel.ShowArchive)
            {
                result.Where(x => !x.IsArchive);
            }
            switch (FilterViewModel.Level)
            {
                case LevelsFilter.Level1:
                    result.Where(Restrictions.IsNull(Projections.Property(() => level2Alias.Id)));
                    break;
                case LevelsFilter.Level2:
                    result.Where(Restrictions.IsNull(Projections.Property(() => level3Alias.Id)));
                    break;
                case LevelsFilter.Level3:
                    result.Where(Restrictions.IsNull(Projections.Property(() => level4Alias.Id)));
                    break;
                case LevelsFilter.Level4:
                    result.Where(Restrictions.IsNull(Projections.Property(() => level5Alias.Id)));
                    break;
            }
        
            result.SelectList(list => list
                .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                .Select(() => level1Alias.Name).WithAlias(() => resultAlias.Level5)
                .Select(() => level2Alias.Name).WithAlias(() => resultAlias.Level4)
                .Select(() => level3Alias.Name).WithAlias(() => resultAlias.Level3)
                .Select(() => level4Alias.Name).WithAlias(() => resultAlias.Level2)
                .Select(() => level5Alias.Name).WithAlias(() => resultAlias.Level1)
                .Select(() => subdivisionAlias.ShortName).WithAlias(() => resultAlias.Subdivision)
                .Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
            ).TransformUsing(Transformers.AliasToBean<ExpenseCategoryJournalNode>())
            .OrderBy(() => level5Alias.Name + level4Alias.Name + level3Alias.Name + level2Alias.Name + level1Alias.Name);

            return result;
        };

        protected override Func<ExpenseCategoryViewModel> CreateDialogFunction => () => new ExpenseCategoryViewModel(
            EntityUoWBuilder.ForCreate(),
            unitOfWorkFactory,
            commonServices,
            fileChooserProvider,
            FilterViewModel

        );
        protected override Func<ExpenseCategoryJournalNode, ExpenseCategoryViewModel> OpenDialogFunction => node => new ExpenseCategoryViewModel(
            EntityUoWBuilder.ForOpen(node.Id),
            unitOfWorkFactory,
            commonServices,
            fileChooserProvider,
            FilterViewModel
        );
        
        protected override void CreatePopupActions()
        {
            base.CreatePopupActions();
            NodeActionsList.Add(new JournalAction("Экспорт", x => true, x => true, selectedItems => {
                var selectedNodes = selectedItems.Cast<ExpenseCategoryJournalNode>();
                StringBuilder CSVbuilder = new StringBuilder();
                foreach (ExpenseCategoryJournalNode expenseCategoryJournalNode in Items)
                {
                    CSVbuilder.Append(expenseCategoryJournalNode.Level1 + ", ");
                    CSVbuilder.Append(expenseCategoryJournalNode.Level2 + ", ");
                    CSVbuilder.Append(expenseCategoryJournalNode.Level3 + ", ");
                    CSVbuilder.Append(expenseCategoryJournalNode.Level4 + ", ");
                    CSVbuilder.Append(expenseCategoryJournalNode.Level5 + ", ");
                    CSVbuilder.Append(expenseCategoryJournalNode.Subdivision + "\n");
                }

                var fileChooserPath = fileChooserProvider.GetExportFilePath();
                var res = CSVbuilder.ToString();

                if (fileChooserPath == "") return;
                Stream fileStream = new FileStream(fileChooserPath, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("Windows-1251")))  
                {  
                    writer.Write("\"sep=,\"\n");
                    writer.Write(res.ToString());
                }                 
                fileChooserProvider.CloseWindow();

            }));

            PopupActionsList.Add(new JournalAction("Архивировать", x => true, x => true, selectedItems => {
                var selectedNodes = selectedItems.Cast<ExpenseCategoryJournalNode>();
                var selectedNode = selectedNodes.FirstOrDefault();
                if(selectedNode != null)
                {
                    selectedNode.IsArchive = true;
                    using (var uow = UnitOfWorkFactory.CreateForRoot<ExpenseCategory>(selectedNode.Id))
                    {
                        uow.Root.SetIsArchiveRecursively(true);
                        uow.Save();                   
                        uow.Commit();
                    }
                }
            }));
        }
    }
}