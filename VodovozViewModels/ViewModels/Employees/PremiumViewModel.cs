using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QSProjectsLib;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class PremiumViewModel : EntityTabViewModelBase<Premium>
	{
		private readonly IEmployeeService employeeService;
		private string employeesSum;
		private PremiumItem selectedItem;

		public PremiumViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices,
			IEmployeeService employeeService, IEmployeeJournalFactory employeeJournalFactory, IPremiumTemplateJournalFactory premiumTemplateJournalFactory)
			: base(uowBuilder, uowFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));

			TabName = Entity.Title;

			CanEdit = PermissionResult.CanUpdate;
			Entity.ObservableItems.ListContentChanged += OnObservableItems_ListContentChanged;
			EmployeeAutocompleteSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			PremiumTemplateAutocompleteSelectorFactory = premiumTemplateJournalFactory.CreatePremiumTemplateAutocompleteSelectorFactory();
		}

		public string EmployeesSum
		{
			get => employeesSum;
			set { SetField(ref employeesSum, value); }
		}
		public bool CanEdit { get; private set; }
		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory PremiumTemplateAutocompleteSelectorFactory { get; }

		#region Commands

		private DelegateCommand addEmployeeCommand;
		public DelegateCommand AddEmployeeCommand
		{
			get
			{
				if(addEmployeeCommand == null)
				{
					addEmployeeCommand = new DelegateCommand(() =>
					{
						var selectorEmployee = EmployeeAutocompleteSelectorFactory.CreateAutocompleteSelector();
						selectorEmployee.OnEntitySelectedResult += OnEmployee_Add;
						this.TabParent.AddSlaveTab(this, selectorEmployee);
					}, () => CanEdit);
					addEmployeeCommand.CanExecuteChangedWith(this, x => x.CanEdit);
				}
				return addEmployeeCommand;
			}
		}

		private DelegateCommand<PremiumItem> selectionChangedCommand;
		public DelegateCommand<PremiumItem> SelectionChangedCommand
		{
			get
			{
				if(selectionChangedCommand == null)
				{
					selectionChangedCommand = new DelegateCommand<PremiumItem>(
						(node) =>
						{
							selectedItem = node;
						},
						(node) => true
					);
				}
				return selectionChangedCommand;
			}
		}

		private DelegateCommand deleteEmployeeCommand;
		public DelegateCommand DeleteEmployeeCommand
		{
			get
			{
				if(deleteEmployeeCommand == null)
				{
					deleteEmployeeCommand = new DelegateCommand(() =>
					{
						var row = selectedItem;
						if(row.Id > 0)
						{
							UoW.Delete(row);
							if(row.WageOperation != null)
								UoW.Delete(row.WageOperation);
						}
						Entity.ObservableItems.Remove(row);
					}, () => CanEdit);
					deleteEmployeeCommand.CanExecuteChangedWith(this, x => x.CanEdit);
				}
				return deleteEmployeeCommand;
			}
		}

		private DelegateCommand divideAtAllCommand;
		public DelegateCommand DivideAtAllCommand => divideAtAllCommand ?? (divideAtAllCommand = new DelegateCommand(
			() =>
			{
				Entity.DivideAtAll();
			},
			() => CanEdit
		));

		private DelegateCommand getReasonFromTemplateCommand;
		public DelegateCommand GetReasonFromTemplate => getReasonFromTemplateCommand ?? (getReasonFromTemplateCommand = new DelegateCommand(
			() =>
			{
				var selectorPremiumTemplate = PremiumTemplateAutocompleteSelectorFactory.CreateAutocompleteSelector();
				selectorPremiumTemplate.OnEntitySelectedResult += OnPremiumTemplate_Select;
				this.TabParent.AddSlaveTab(this, selectorPremiumTemplate);
			},
			() => CanEdit
		));

		#endregion

		#region Events
		private void OnEmployee_Add(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedEmplyeeNode = e.SelectedNodes.FirstOrDefault();
			if(selectedEmplyeeNode == null)
			{
				return;
			}

			var employee = UoW.GetById<Employee>(selectedEmplyeeNode.Id);
			if(Entity.Items.Any(x => x.Employee.Id == employee.Id))
			{
				ShowErrorMessage($"Сотрудник {employee.ShortName} уже присутствует в списке.");
				return;
			}
			Entity.AddItem(employee);
		}

		private void OnPremiumTemplate_Select(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedEmplyeeNode = e.SelectedNodes.Cast<PremiumTemplateJournalNode>().FirstOrDefault();

			if(selectedEmplyeeNode == null)
			{
				return;
			}

			Entity.PremiumReasonString = selectedEmplyeeNode.Reason;
			Entity.TotalMoney = selectedEmplyeeNode.PremiumMoney;
		}

		private void OnObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			decimal sum = Entity.Items.Sum(x => x.Money);
			EmployeesSum = $"Итого по сотрудникам: {CurrencyWorks.GetShortCurrencyString(sum)}";
		}

		#endregion

		private bool GetAuthor(out Employee cashier)
		{
			cashier = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);
			if(cashier == null)
			{
				ShowErrorMessage("Ваш пользователь не привязан к действующему сотруднику.");
				return false;
			}
			return true;
		}

		public override bool Save(bool close)
		{
			if(!CanEdit)
				return false;

			Employee author;
			if(!GetAuthor(out author))
				return false;

			if(Entity.Author == null)
			{
				Entity.Author = author;
			}

			Entity.UpdateWageOperations(UoW);

			return base.Save(close);
		}
	}
}
