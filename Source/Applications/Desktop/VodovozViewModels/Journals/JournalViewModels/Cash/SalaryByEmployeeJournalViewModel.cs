using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class SalaryByEmployeeJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Employee, EmployeeViewModel, EmployeeJournalNode, SalaryByEmployeeJournalFilterViewModel>
	{
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public SalaryByEmployeeJournalViewModel(
			SalaryByEmployeeJournalFilterViewModel filterViewModel,
			IGtkTabsOpener gtkTabsOpener,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал выдач З/П";

			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			UpdateOnChanges(
				typeof(Employee),
				typeof(WagesMovementOperations)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			var addAction = new JournalAction("Добавить расходный ордер",
				selected => true,
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<EmployeeJournalNode>();
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

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) =>
		{
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeJournalNode resultAlias = null;
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

			var wageQuery = QueryOver.Of(() => wageAlias)
				.Where(wage => wage.Employee.Id == employeeAlias.Id)
				.Select(Projections.Sum(Projections.Property(() => wageAlias.Money)));

			if(FilterViewModel?.MinBalance != null)
			{
				wageQuery.Where(() => wageAlias.Money < FilterViewModel.MinBalance);
			}

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
				)
				.OrderBy(e => e.LastName).Asc
				.OrderBy(e => e.Name).Asc
				.OrderBy(e => e.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());

			return employeesQuery;
		};

		protected override Func<EmployeeViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание сотрудника из журнала");

		protected override Func<EmployeeJournalNode, EmployeeViewModel> OpenDialogFunction => (node) =>
			throw new NotSupportedException("Не поддерживается изменение сотрудника из журнала");
	}
}
