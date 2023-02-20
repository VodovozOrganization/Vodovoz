using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Reports.SalaryByEmployeeJournalReport;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class SalaryByEmployeeJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Employee, EmployeeViewModel, EmployeeWithLastWorkingDayJournalNode, SalaryByEmployeeJournalFilterViewModel>
	{
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IFileDialogService _fileDialogService;

		public SalaryByEmployeeJournalViewModel(
			SalaryByEmployeeJournalFilterViewModel filterViewModel,
			IGtkTabsOpener gtkTabsOpener,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFileDialogService fileDialogService,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал выдач З/П";

			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			UpdateOnChanges(
				typeof(Employee),
				typeof(WagesMovementOperations)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAddAction();
			CreateExportAction();
		}

		private void CreateAddAction()
		{
			var addAction = new JournalAction("Добавить расходный ордер",
				selected => true,
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<EmployeeWithLastWorkingDayJournalNode>();
					var node = selectedNodes.FirstOrDefault();
					if(node == null)
					{
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран сотрудник");
					}
					else
					{
						_gtkTabsOpener.OpenCashExpenseDlg(master: this, node.Id, node.Balance, canChangeEmployee: false, ExpenseType.Salary);
					}
				},
				hotKeys: "Insert");
			RowActivatedAction = addAction;
			NodeActionsList.Add(addAction);
		}

		private void CreateExportAction()
		{
			NodeActionsList.Add(new JournalAction("Экспорт в Excel", x => true, x => true,
				selectedItems =>
				{
					var nodes = ItemsSourceQueryFunction(UoW).List<EmployeeWithLastWorkingDayJournalNode>();

					var report = new SalaryByEmployeeJournalReport(nodes, _fileDialogService);
					report.Export();
				}));
		}

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) =>
		{
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeWithLastWorkingDayJournalNode resultAlias = null;
			WagesMovementOperations wageAlias = null;

			var employeesQuery = uow.Session.QueryOver(() => employeeAlias)
				.Left.JoinAlias(e => e.Subdivision, () => subdivisionAlias);

			if(FilterViewModel?.Status != null)
			{
				employeesQuery.Where(e => e.Status == FilterViewModel.Status);
			}

			if(FilterViewModel?.Category != null)
			{
				employeesQuery.Where(e => e.Category == FilterViewModel.Category);
			}

			if(FilterViewModel?.Subdivision != null)
			{
				employeesQuery.Where(e => e.Subdivision == FilterViewModel.Subdivision);
			}

			if(FilterViewModel?.MinBalance != null)
			{
				var query = uow.Session.Query<WagesMovementOperations>()
				.GroupBy(w => w.Employee.Id)
				.Where(s => s.Sum(w => w.Money) < FilterViewModel.MinBalance)
				.Select(w => w.Key)
				.ToArray();

				employeesQuery.Where(Restrictions.In(nameof(Employee.Id), query));
			}

			var wageQuery = QueryOver.Of(() => wageAlias)
				.Where(wage => wage.Employee.Id == employeeAlias.Id)
				.Select(Projections.Sum(Projections.Property(() => wageAlias.Money)));

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
			);

			employeesQuery.Where(GetSearchCriterion(
				() => employeeAlias.Id,
				() => employeeProjection
			));

			employeesQuery
				.SelectList(list => list
					.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
					.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
					.Select(() => employeeAlias.Comment).WithAlias(() => resultAlias.EmployeeComment)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionTitle)
					.SelectSubQuery(wageQuery).WithAlias(() => resultAlias.Balance)
				);

			employeesQuery
				//.OrderBy(e => e.Id).Asc
				.OrderBy(e => e.LastName).Asc
				.OrderBy(e => e.Name).Asc
				.OrderBy(e => e.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeWithLastWorkingDayJournalNode>());

			return employeesQuery;
		};

		protected override Func<EmployeeViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание сотрудника из журнала");

		protected override Func<EmployeeWithLastWorkingDayJournalNode, EmployeeViewModel> OpenDialogFunction => (node) =>
			throw new NotSupportedException("Не поддерживается изменение сотрудника из журнала");
	}
}
