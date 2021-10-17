using System;
using System.IO;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class ExpenseCategoryJournalViewModel : FilterableSingleEntityJournalViewModelBase
	<
		ExpenseCategory,
		ExpenseCategoryViewModel,
		ExpenseCategoryJournalNode,
		ExpenseCategoryJournalFilterViewModel
	>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFileChooserProvider _fileChooserProvider;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IExpenseCategorySelectorFactory _expenseCategorySelectorFactory;

		public ExpenseCategoryJournalViewModel(
			ExpenseCategoryJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFileChooserProvider fileChooserProvider,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_fileChooserProvider = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_expenseCategorySelectorFactory =
				expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));

			TabName = "Категории расхода";

			UpdateOnChanges(
				typeof(ExpenseCategory),
				typeof(Subdivision)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<ExpenseCategory>> ItemsSourceQueryFunction => uow =>
		{
			ExpenseCategoryJournalNode resultAlias = null;

			ExpenseCategory level1Alias = null;
			ExpenseCategory level2Alias = null;
			ExpenseCategory level3Alias = null;
			ExpenseCategory level4Alias = null;
			ExpenseCategory level5Alias = null;
			Subdivision subdivisionAlias = null;

			var query = uow.Session.QueryOver(() => level1Alias)
				.Left.JoinAlias(() => level1Alias.Parent, () => level2Alias)
				.Left.JoinAlias(() => level2Alias.Parent, () => level3Alias)
				.Left.JoinAlias(() => level3Alias.Parent, () => level4Alias)
				.Left.JoinAlias(() => level4Alias.Parent, () => level5Alias)
				.Left.JoinAlias(() => level1Alias.Subdivision, () => subdivisionAlias);

			if(!FilterViewModel.ShowArchive)
			{
				query.Where(x => !x.IsArchive);
			}

			switch(FilterViewModel.Level)
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
					.Select(() => subdivisionAlias.ShortName).WithAlias(() => resultAlias.Subdivision)
					.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
				).TransformUsing(Transformers.AliasToBean<ExpenseCategoryJournalNode>())
				.OrderBy(() => level5Alias.Name + level4Alias.Name + level3Alias.Name + level2Alias.Name + level1Alias.Name);

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

		protected override Func<ExpenseCategoryViewModel> CreateDialogFunction => () =>
			new ExpenseCategoryViewModel(
				EntityUoWBuilder.ForCreate(),
				_unitOfWorkFactory,
				commonServices,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_expenseCategorySelectorFactory
			);

		protected override Func<ExpenseCategoryJournalNode, ExpenseCategoryViewModel> OpenDialogFunction => node =>
			new ExpenseCategoryViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				_unitOfWorkFactory,
				commonServices,
				_employeeJournalFactory,
				_subdivisionJournalFactory,
				_expenseCategorySelectorFactory
			);

		protected override void CreatePopupActions()
		{
			base.CreatePopupActions();
			NodeActionsList.Add(new JournalAction("Экспорт", x => true, x => true, selectedItems =>
			{
				StringBuilder CSVbuilder = new StringBuilder();
				foreach(ExpenseCategoryJournalNode expenseCategoryJournalNode in Items)
				{
					CSVbuilder.Append(expenseCategoryJournalNode.Level1 + ", ");
					CSVbuilder.Append(expenseCategoryJournalNode.Level2 + ", ");
					CSVbuilder.Append(expenseCategoryJournalNode.Level3 + ", ");
					CSVbuilder.Append(expenseCategoryJournalNode.Level4 + ", ");
					CSVbuilder.Append(expenseCategoryJournalNode.Level5 + ", ");
					CSVbuilder.Append(expenseCategoryJournalNode.Subdivision + "\n");
				}

				var fileChooserPath = _fileChooserProvider.GetExportFilePath($"Категории расхода {DateTime.Now.ToShortDateString()}");
				var res = CSVbuilder.ToString();

				if(fileChooserPath == "") return;
				Stream fileStream = new FileStream(fileChooserPath, FileMode.Create);
				using(StreamWriter writer = new StreamWriter(fileStream, Encoding.GetEncoding("Windows-1251")))
				{
					writer.Write("\"sep=,\"\n");
					writer.Write(res);
				}

				_fileChooserProvider.CloseWindow();
			}));

			PopupActionsList.Add(new JournalAction("Архивировать", x => true, x => true, selectedItems =>
			{
				var selectedNodes = selectedItems.Cast<ExpenseCategoryJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null)
				{
					selectedNode.IsArchive = true;
					using(var uow = UnitOfWorkFactory.CreateForRoot<ExpenseCategory>(selectedNode.Id))
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
