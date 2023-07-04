using System;
using System.Collections.Generic;
using System.Linq;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Employees;
using QS.Commands;
using Vodovoz.ViewModelBased;
using QS.DomainModel.NotifyChange;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;

namespace Vodovoz.Dialogs.Cash.CashTransfer
{
	public class CommonCashTransferDocumentViewModel : ViewModel<CommonCashTransferDocument>
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		
		private IEnumerable<Subdivision> cashSubdivisions;
		private IList<Subdivision> availableSubdivisionsForUser;

		public CommonCashTransferDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory factory,
			ICategoryRepository categoryRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory) : base(entityUoWBuilder, factory)
		{
			_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			if(entityUoWBuilder.IsNewEntity) {
				Entity.CreationDate = DateTime.Now;
				Entity.Author = Cashier;
			}
			
			CreateCommands();
			UpdateCashSubdivisions();
			UpdateIncomeCategories();
			UpdateExpenseCategories();
			View = new CommonCashTransferDlg(this);

			Entity.PropertyChanged += Entity_PropertyChanged;

			ConfigureEntityChangingRelations();
			ConfigEntityUpdateSubscribes();
		}

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		private Employee cashier;
		public Employee Cashier {
			get {
				if(cashier == null) {
					cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				}
				return cashier;
			}
		}

		public bool CanEdit => Entity.Status == CashTransferDocumentStatuses.New;

		public override bool Save()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			return base.Save();
		}

		protected void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEdit
			);
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.CashSubdivisionFrom)) {
				UpdateSubdivisionsTo();
			}
			if(e.PropertyName == nameof(Entity.CashSubdivisionTo)) {
				UpdateSubdivisionsFrom();
			}
		}

		#region Подписки на внешние измнения сущностей

		private void ConfigEntityUpdateSubscribes()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<IncomeCategory>(IncomeCategoryEntityConfig_EntityUpdated);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<ExpenseCategory>(ExpenseCategoryEntityConfig_EntityUpdated);
		}

		private void IncomeCategoryEntityConfig_EntityUpdated(EntityChangeEvent[] changeEvents)
		{
			foreach(var updatedItem in changeEvents.Select(x => x.Entity)) {
				IncomeCategory updatedIncomeCategory = updatedItem as IncomeCategory;
				//Если хотябы одна необходимая статья обновилась можем обнлять весь список.
				if(updatedIncomeCategory != null && updatedIncomeCategory.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeTransferDocument) {
					UpdateIncomeCategories();
					return;
				}
			}
		}

		private void ExpenseCategoryEntityConfig_EntityUpdated(EntityChangeEvent[] changeEvents)
		{
			foreach(var updatedItem in changeEvents.Select(x => x.Entity)) {
				ExpenseCategory updatedExpenseCategory = updatedItem as ExpenseCategory;
				//Если хотябы одна необходимая статья обновилась можем обнлять весь список.
				if(updatedExpenseCategory != null && updatedExpenseCategory.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseTransferDocument) {
					UpdateExpenseCategories();
					return;
				}
			}
		}

		#endregion Подписки на внешние измнения сущностей

		#region Commands

		public DelegateCommand SendCommand { get; private set; }
		public DelegateCommand ReceiveCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }


		private void CreateCommands()
		{
			SendCommand = new DelegateCommand(
				() => {
					var validator = new ObjectValidator(new GtkValidationViewFactory());
					if(!validator.Validate(Entity))
					{
						return;
					}

					Entity.Send(Cashier, Entity.Comment);
				},
				() => {
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.Driver != null
						&& Entity.Car != null
						&& Entity.CashSubdivisionFrom != null
						&& Entity.CashSubdivisionTo != null
						&& Entity.ExpenseCategory != null
						&& Entity.IncomeCategory != null
						&& Entity.TransferedSum > 0;
				}
			);
			SendCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Driver,
				x => x.Car,
				x => x.CashSubdivisionFrom,
				x => x.CashSubdivisionTo,
				x => x.TransferedSum,
				x => x.ExpenseCategory,
				x => x.IncomeCategory
			);

			ReceiveCommand = new DelegateCommand(
				() => {
					Entity.Receive(Cashier, Entity.Comment);
				},
				() => {
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.Sent
						&& availableSubdivisionsForUser.Contains(Entity.CashSubdivisionTo)
						&& Entity.Id != 0;
				}
			);
			ReceiveCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Id
			);

			PrintCommand = new DelegateCommand(
				() => {
					var reportInfo = new QS.Report.ReportInfo {
						Title = String.Format($"Документ перемещения №{Entity.Id} от {Entity.CreationDate:d}"),
						Identifier = "Documents.CommonCashTransfer",
						Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } }
					};

					var report = new QSReport.ReportViewDlg(reportInfo);
					View.TabParent.AddTab(report, View, false);
				},
				() => Entity.Id != 0
			);
		}

		#endregion Commands

		#region Настройка списков статей дохода и прихода

		private IList<IncomeCategory> incomeCategories;
		public virtual IList<IncomeCategory> IncomeCategories {
			get => incomeCategories;
			set => SetField(ref incomeCategories, value, () => IncomeCategories);
		}

		private IList<ExpenseCategory> expenseCategories;
		public virtual IList<ExpenseCategory> ExpenseCategories {
			get => expenseCategories;
			set => SetField(ref expenseCategories, value, () => ExpenseCategories);
		}

		private void UpdateIncomeCategories()
		{
			if(!CanEdit) {
				return;
			}
			var currentSelectedCategory = Entity.IncomeCategory;
			IncomeCategories = 
				_categoryRepository.IncomeCategories(UoW)
					.Where(x => x.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeTransferDocument).ToList();
			if(IncomeCategories.Contains(currentSelectedCategory)) {
				Entity.IncomeCategory = currentSelectedCategory;
			}
		}

		private void UpdateExpenseCategories()
		{
			if(!CanEdit) {
				return;
			}
			var currentSelectedCategory = Entity.ExpenseCategory;
			ExpenseCategories =
				_categoryRepository.ExpenseCategories(UoW)
					.Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseTransferDocument).ToList();
			if(ExpenseCategories.Contains(currentSelectedCategory)) {
				Entity.ExpenseCategory = currentSelectedCategory;
			}
		}

		#endregion Настройка списков статей дохода и прихода

		#region Настройка списков доступных подразделений кассы

		private IEnumerable<Subdivision> subdivisionsFrom;
		public virtual IEnumerable<Subdivision> SubdivisionsFrom {
			get => subdivisionsFrom;
			set => SetField(ref subdivisionsFrom, value, () => SubdivisionsFrom);
		}

		private IEnumerable<Subdivision> subdivisionsTo;
		public virtual IEnumerable<Subdivision> SubdivisionsTo {
			get => subdivisionsTo;
			set => SetField(ref subdivisionsTo, value, () => SubdivisionsTo);
		}

		private void UpdateCashSubdivisions()
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			availableSubdivisionsForUser = _subdivisionRepository.GetAvailableSubdivionsForUser(UoW, cashDocumentTypes).ToList();
			
			if(Entity.Id != 0
			   && !CanEdit
			   && Entity.CashSubdivisionFrom != null
			   && !availableSubdivisionsForUser.Contains(Entity.CashSubdivisionFrom))
			{
				availableSubdivisionsForUser.Add(Entity.CashSubdivisionFrom);
			}
			
			cashSubdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, cashDocumentTypes).Distinct();
			SubdivisionsFrom = availableSubdivisionsForUser;
			SubdivisionsTo = cashSubdivisions;
		}


		private bool isUpdatingSubdivisions = false;

		private void UpdateSubdivisionsFrom()
		{
			if(isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonFrom = Entity.CashSubdivisionFrom;
			SubdivisionsFrom = availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo);
			if(SubdivisionsTo.Contains(currentSubdivisonFrom)) {
				Entity.CashSubdivisionFrom = currentSubdivisonFrom;
			}
			isUpdatingSubdivisions = false;
		}

		private void UpdateSubdivisionsTo()
		{
			if(isUpdatingSubdivisions) {
				return;
			}
			isUpdatingSubdivisions = true;
			var currentSubdivisonTo = Entity.CashSubdivisionTo;
			SubdivisionsTo = cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom);
			if(SubdivisionsTo.Contains(currentSubdivisonTo)) {
				Entity.CashSubdivisionTo = currentSubdivisonTo;
			}
			isUpdatingSubdivisions = false;
		}

		#endregion Настройка списков доступных подразделений кассы
	}
}
