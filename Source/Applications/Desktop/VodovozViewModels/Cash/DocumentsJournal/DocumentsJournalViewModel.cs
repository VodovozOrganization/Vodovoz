using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.NHibernateProjections.Cash;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools;
using static Vodovoz.ViewModels.Cash.DocumentsJournal.DocumentsJournalViewModel;

namespace Vodovoz.ViewModels.Cash.DocumentsJournal
{
	public partial class DocumentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<DocumentNode, DocumentsFilterViewModel>,
		IDocumentsInfoProvider
	{
		private readonly IDictionary<Type, IPermissionResult> _domainObjectsPermissions;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly bool _hasAccessToHiddenFinancialCategories;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public DocumentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			DocumentsFilterViewModel filterViewModel,
			Action<DocumentsFilterViewModel> filterAction = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			Title = "Журнал кассовых документов";

			DomainObjectsTypes = new[]
			{
				typeof(Income),
				typeof(Expense),
				typeof(AdvanceReport)
			};

			FilterViewModel.DomainObjectsTypes = DomainObjectsTypes;

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));

			_domainObjectsPermissions = InitializePermissionsMatrix(DomainObjectsTypes);

			_hasAccessToHiddenFinancialCategories =
				_currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.FinancialCategory.HasAccessToHiddenFinancialCategories);

			FilterViewModel.JournalViewModel = this;

			UpdateOnChanges(DomainObjectsTypes.Concat(
				new Type[]
				{
					typeof(FinancialIncomeCategory),
					typeof(FinancialExpenseCategory),
					typeof(Employee),
					typeof(Subdivision)
				}).ToArray());

			RegisterDocuments();

			UpdateAllEntityPermissions();

			CreateNodeActions();
			CreatePopupActions();

			DataLoader.DynamicLoadingEnabled = false;
			DataLoader.ItemsListUpdated += OnItemsUpdated;

			if(filterAction != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterAction);
			}
		}

		private void OnItemsUpdated(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Items));

			OnPropertyChanged(nameof(FooterInfo));
		}

		public Type[] DomainObjectsTypes { get; }

		public int[] AvailableSubdivisionsIds => FilterViewModel.AvailableSubdivisions?.Select(x => x.Id).ToArray() ?? Array.Empty<int>();

		public string TotalSumString =>
			CurrencyWorks.GetShortCurrencyString(
				((ICollection<DocumentNode>)Items).Sum(x => x.MoneySigned));

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();

			PopupActionsList.Add(new JournalAction("Повторить РКО",
				(selectedItems) => selectedItems
					.Any(x => x is DocumentNode node
						&& node.CashDocumentType == CashDocumentType.Expense
						&& node.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoice),
				(selectedItems) => true,
				(selectedItems) =>
				{
					var selectedNodes = selectedItems.Cast<DocumentNode>();
					var selectedNode = selectedNodes.FirstOrDefault();

					if(selectedNode != null)
					{
						var page = NavigationManager.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
						page.ViewModel.CopyFromExpense(selectedNode.Id);
					}
				}));
		}

		private IDictionary<Type, IPermissionResult> InitializePermissionsMatrix(IEnumerable<Type> types)
		{
			var result = new Dictionary<Type, IPermissionResult>();

			foreach(var domainObject in types)
			{
				result.Add(domainObject, _currentPermissionService.ValidateEntityPermission(domainObject));
			}

			return result;
		}

		public override string FooterInfo => $"Сумма выбранных документов: {TotalSumString}. " +
			$"Загружено: {Items.Count} шт.";

		public DocumentsFilterViewModel DocumentsFilterViewModel => FilterViewModel;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.CashInfoPanelView };

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateOpenDocumentAction();
			CreateDefaultDeleteAction();
		}

		private void CreateAddActions()
		{
			var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });

			var incomeCreateNodeAction = new JournalAction(
				typeof(Income).GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
				(selected) => _domainObjectsPermissions[typeof(Income)].CanCreate,
				(selected) => _domainObjectsPermissions[typeof(Income)].CanCreate,
				(selected) => NavigationManager.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()));

			addParentNodeAction.ChildActionsList.Add(incomeCreateNodeAction);

			var incomeSelfDeliveryCreateNodeAction = new JournalAction(
				typeof(Income).GetClassUserFriendlyName().Accusative.CapitalizeSentence() + " самовывоз",
				(selected) => _domainObjectsPermissions[typeof(Income)].CanCreate,
				(selected) => _domainObjectsPermissions[typeof(Income)].CanCreate,
				(selected) => NavigationManager.OpenViewModel<IncomeSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()));

			addParentNodeAction.ChildActionsList.Add(incomeSelfDeliveryCreateNodeAction);

			var expenseCreateNodeAction = new JournalAction(
				typeof(Expense).GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
				(selected) => _domainObjectsPermissions[typeof(Expense)].CanCreate,
				(selected) => _domainObjectsPermissions[typeof(Expense)].CanCreate,
				(selected) => NavigationManager.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()));

			addParentNodeAction.ChildActionsList.Add(expenseCreateNodeAction);

			var expenseSelfDeliveryCreateNodeAction = new JournalAction(
				typeof(Expense).GetClassUserFriendlyName().Accusative.CapitalizeSentence() + " самовывоз",
				(selected) => _domainObjectsPermissions[typeof(Expense)].CanCreate,
				(selected) => _domainObjectsPermissions[typeof(Expense)].CanCreate,
				(selected) => NavigationManager.OpenViewModel<ExpenseSelfDeliveryViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()));

			addParentNodeAction.ChildActionsList.Add(expenseSelfDeliveryCreateNodeAction);

			var advanceReportCreateNodeAction = new JournalAction(
				typeof(AdvanceReport).GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
				(selected) => _domainObjectsPermissions[typeof(AdvanceReport)].CanCreate,
				(selected) => _domainObjectsPermissions[typeof(AdvanceReport)].CanCreate,
				(selected) => NavigationManager.OpenViewModel<AdvanceReportViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()));

			addParentNodeAction.ChildActionsList.Add(advanceReportCreateNodeAction);

			NodeActionsList.Add(addParentNodeAction);
		}

		private void CreateOpenDocumentAction()
		{
			var editAction = new JournalAction(
				"Редактировать",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DocumentNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					DocumentNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanRead || config.PermissionResult.CanUpdate;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<DocumentNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					DocumentNode selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					var foundDocumentConfig = config.EntityDocumentConfigurations
						.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);

					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				});

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}

		private void RegisterDocuments()
		{
			RegisterEntity(GetQueryAdvanceReport)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager
						.OpenViewModel<AdvanceReportViewModel, IEntityUoWBuilder>(
							this,
							EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(AdvanceReport),
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false })
				.FinishConfiguration();

			RegisterEntity(GetQueryIncome)
				.AddDocumentConfiguration(
					() => null,
					(selected) =>
					{
						var id = selected.Id;

						if(selected.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery)
						{
							return NavigationManager
								.OpenViewModel<IncomeSelfDeliveryViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(id)).ViewModel;
						}

						if(selected.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeTransferDocument)
						{
							return NavigationManager
								.OpenViewModel<TransferIncomeViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(id)).ViewModel;
						}

						return NavigationManager
							.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(
								this,
								EntityUoWBuilder.ForOpen(id)).ViewModel;
					},
					(node) => node.EntityType == typeof(Income),
					journalParameters: new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false })
				.FinishConfiguration();

			RegisterEntity(GetQueryExpense)
				.AddDocumentConfiguration(
					() => null,
					(selected) =>
					{
						var id = selected.Id;

						if(selected.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
						{
							return NavigationManager
								.OpenViewModel<ExpenseSelfDeliveryViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(id)).ViewModel;
						}

						if(selected.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseTransferDocument)
						{
							return NavigationManager
								.OpenViewModel<TransferExpenseViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(id)).ViewModel;
						}

						return NavigationManager
							.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(
								this,
								EntityUoWBuilder.ForOpen(id)).ViewModel;
					},
					(node) => node.EntityType == typeof(Expense),
					journalParameters: new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false })
				.FinishConfiguration();

			var dataLoader = DataLoader as ThreadDataLoader<DocumentNode>;
			dataLoader.MergeInOrderBy(node => node.Date, true);
		}

		private IQueryOver<Income> GetQueryIncome(IUnitOfWork unitOfWork)
		{
			DocumentNode resultAlias = null;

			Income incomeAlias = null;
			Employee employeeAlias = null;
			Employee casherAlias = null;

			FinancialExpenseCategory financialExpenseCategoryAlias = null;
			FinancialIncomeCategory financialIncomeCategoryAlias = null;

			var incomeDocumentTypes = new CashDocumentType[]
			{
				CashDocumentType.Income,
				CashDocumentType.IncomeSelfDelivery
			};

			var query = unitOfWork.Session.QueryOver(() => incomeAlias);

			if((FilterViewModel.CashDocumentType is null || incomeDocumentTypes.Contains(FilterViewModel.CashDocumentType.Value))
				&& (FilterViewModel.RestrictDocument is null || FilterViewModel.RestrictDocument == typeof(Income)))
			{
				if(FilterViewModel.Subdivision is null)
				{
					query.WhereRestrictionOn(x => x.RelatedToSubdivision.Id)
						.IsIn(AvailableSubdivisionsIds);
				}
				else
				{
					query.Where(x => x.RelatedToSubdivision.Id == FilterViewModel.Subdivision.Id);
				}

				if(FilterViewModel.CashDocumentType != null)
				{
					IncomeInvoiceDocumentType documentType = IncomeInvoiceDocumentType.IncomeInvoice;

					if(FilterViewModel.CashDocumentType.Value == CashDocumentType.IncomeSelfDelivery)
					{
						documentType = IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery;
					}

					query.Where(income => income.TypeDocument == documentType);
				}

				if(FilterViewModel.FinancialExpenseCategory != null)
				{
					query.Where(income => income.ExpenseCategoryId == FilterViewModel.FinancialExpenseCategory.Id);
				}

				if(FilterViewModel.FinancialIncomeCategory != null)
				{
					query.Where(income => income.IncomeCategoryId == FilterViewModel.FinancialIncomeCategory.Id);
				}

				if(FilterViewModel.StartDate != null)
				{
					query.Where(income => income.Date >= FilterViewModel.StartDate.Value);
				}

				if(FilterViewModel.EndDate != null)
				{
					query.Where(income => income.Date <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.Employee != null)
				{
					query.Where(income => income.Employee == FilterViewModel.Employee);
				}

				if(FilterViewModel.HiddenIncomes.Any())
				{
					query.Where(Restrictions.Not(Restrictions.In(Projections.Property(() => incomeAlias.Id), FilterViewModel.HiddenIncomes)));
				}

				if(FilterViewModel.RestrictRelatedToSubdivisionId != null)
				{
					query.Where(income => income.RelatedToSubdivision.Id == FilterViewModel.RestrictRelatedToSubdivisionId);
				}

				if(FilterViewModel.RestrictNotTransfered)
				{
					query.Where(income => income.TransferedBy == null);
				}

				query.Where(GetSearchCriterion(
					() => incomeAlias.Id,
					() => incomeAlias.Description,
					() => employeeAlias.Name,
					() => employeeAlias.LastName,
					() => employeeAlias.Patronymic,
					() => incomeAlias.Date,
					() => casherAlias.Name,
					() => casherAlias.LastName,
					() => casherAlias.Patronymic,
					() => incomeAlias.Money,
					() => financialExpenseCategoryAlias.Title,
					() => financialIncomeCategoryAlias.Title));
			}
			else
			{
				query.Where(() => incomeAlias.Id == -1);
			}

			query.Left.JoinAlias(() => incomeAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => incomeAlias.Casher, () => casherAlias)
				.JoinEntityAlias(
					() => financialIncomeCategoryAlias,
					() => incomeAlias.IncomeCategoryId == financialIncomeCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => financialExpenseCategoryAlias,
					() => incomeAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(!_hasAccessToHiddenFinancialCategories)
			{
				query.Where(() =>
					(incomeAlias.IncomeCategoryId == null 
					|| (incomeAlias.IncomeCategoryId != null && !financialIncomeCategoryAlias.IsHiddenFromPublicAccess)
					|| (incomeAlias.IncomeCategoryId != null && !financialExpenseCategoryAlias.IsHiddenFromPublicAccess)
					));
			}

			query.SelectList(list => list
					.Select(() => incomeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(IncomeProjections.GetTitleProjection()).WithAlias(() => resultAlias.Name)
					.Select(() => incomeAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(() => incomeAlias.Money).WithAlias(() => resultAlias.Money)
					.Select(() => incomeAlias.Description).WithAlias(() => resultAlias.Description)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => typeof(Income)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.Eq(
							Projections.Property(
								() => incomeAlias.TypeDocument),
								IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery),
						Projections.Constant(CashDocumentType.IncomeSelfDelivery),
						Projections.Constant(CashDocumentType.Income)))
					.WithAlias(() => resultAlias.CashDocumentType)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => casherAlias.Name).WithAlias(() => resultAlias.CasherName)
					.Select(() => casherAlias.LastName).WithAlias(() => resultAlias.CasherSurname)
					.Select(() => casherAlias.Patronymic).WithAlias(() => resultAlias.CasherPatronymic)
					.Select(Projections.SqlFunction(
							"COALESCE",
							NHibernateUtil.String,
							Projections.Property(() => financialIncomeCategoryAlias.Title),
							Projections.Property(() => financialExpenseCategoryAlias.Title)))
					.WithAlias(() => resultAlias.Category)
					.Select(() => incomeAlias.TypeDocument).WithAlias(() => resultAlias.IncomeDocumentType));

			return query
				.OrderBy(x => x.Date).Desc
				.TransformUsing(Transformers.AliasToBean<DocumentNode>());
		}

		private IQueryOver<Expense> GetQueryExpense(IUnitOfWork unitOfWork)
		{
			DocumentNode resultAlias = null;

			Expense expenseAlias = null;
			Employee employeeAlias = null;
			Employee casherAlias = null;

			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			var expenseDocumentTypes = new CashDocumentType[]
			{
				CashDocumentType.Expense,
				CashDocumentType.ExpenseSelfDelivery
			};

			var query = unitOfWork.Session.QueryOver(() => expenseAlias);

			if(FilterViewModel.FinancialIncomeCategory == null
				&& (FilterViewModel.CashDocumentType == null || expenseDocumentTypes.Contains(FilterViewModel.CashDocumentType.Value))
				&& (FilterViewModel.RestrictDocument is null || FilterViewModel.RestrictDocument == typeof(Expense)))
			{
				if(FilterViewModel.Subdivision is null)
				{
					query.WhereRestrictionOn(x => x.RelatedToSubdivision.Id)
						.IsIn(AvailableSubdivisionsIds);
				}
				else
				{
					query.Where(x => x.RelatedToSubdivision.Id == FilterViewModel.Subdivision.Id);
				}

				if(FilterViewModel.CashDocumentType != null)
				{
					var documentType = ExpenseInvoiceDocumentType.ExpenseInvoice;

					if(FilterViewModel.CashDocumentType.Value == CashDocumentType.ExpenseSelfDelivery)
					{
						documentType = ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;
					}

					query.Where(expense => expense.TypeDocument == documentType);
				}

				if(FilterViewModel.FinancialExpenseCategory != null)
				{
					query.Where(expense => expense.ExpenseCategoryId == FilterViewModel.FinancialExpenseCategory.Id);
				}

				if(FilterViewModel.StartDate.HasValue)
				{
					query.Where(expense => expense.Date >= FilterViewModel.StartDate.Value);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					query.Where(expense => expense.Date <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.Employee != null)
				{
					query.Where(expense => expense.Employee == FilterViewModel.Employee);
				}

				if(FilterViewModel.HiddenExpenses.Any())
				{
					query.Where(Restrictions.Not(Restrictions.In(Projections.Property(() => expenseAlias.Id), FilterViewModel.HiddenExpenses)));
				}

				if(FilterViewModel.RestrictNotTransfered)
				{
					query.Where(expense => expense.TransferedBy == null);
				}

				query.Where(GetSearchCriterion(
					() => expenseAlias.Id,
					() => expenseAlias.Description,
					() => employeeAlias.Name,
					() => employeeAlias.LastName,
					() => employeeAlias.Patronymic,
					() => expenseAlias.Date,
					() => casherAlias.Name,
					() => casherAlias.LastName,
					() => casherAlias.Patronymic,
					() => expenseAlias.Money,
					() => financialExpenseCategoryAlias.Title));
			}
			else
			{
				query.Where(() => expenseAlias.Id == -1);
			}

			query.Left.JoinAlias(() => expenseAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => expenseAlias.Casher, () => casherAlias)
				.JoinEntityAlias(
					() => financialExpenseCategoryAlias,
					() => expenseAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(!_hasAccessToHiddenFinancialCategories)
			{
				query.Where(() =>
					(expenseAlias.ExpenseCategoryId == null
					|| (expenseAlias.ExpenseCategoryId != null && !financialExpenseCategoryAlias.IsHiddenFromPublicAccess)
					));
			}
			
			query.SelectList(list => list
					.Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(ExpenseProjections.GetTitleProjection()).WithAlias(() => resultAlias.Name)
					.Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(() => expenseAlias.Money).WithAlias(() => resultAlias.Money)
					.Select(() => expenseAlias.Description).WithAlias(() => resultAlias.Description)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => typeof(Expense)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.Eq(
							Projections.Property(
								() => expenseAlias.TypeDocument),
								ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery),
						Projections.Constant(CashDocumentType.ExpenseSelfDelivery),
						Projections.Constant(CashDocumentType.Expense)))
					.WithAlias(() => resultAlias.CashDocumentType)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => casherAlias.Name).WithAlias(() => resultAlias.CasherName)
					.Select(() => casherAlias.LastName).WithAlias(() => resultAlias.CasherSurname)
					.Select(() => casherAlias.Patronymic).WithAlias(() => resultAlias.CasherPatronymic)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.Category)
					.Select(() => expenseAlias.TypeDocument).WithAlias(() => resultAlias.ExpenseDocumentType));

			return query
				.OrderBy(x => x.Date).Desc
				.TransformUsing(Transformers.AliasToBean<DocumentNode>());
		}

		private IQueryOver<AdvanceReport> GetQueryAdvanceReport(IUnitOfWork unitOfWork)
		{
			AdvanceReport advanceReportAlias = null;

			Employee employeeAlias = null;
			Employee casherAlias = null;

			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			DocumentNode resultAlias = null;

			var query = unitOfWork.Session.QueryOver(() => advanceReportAlias);

			if(FilterViewModel.FinancialIncomeCategory == null
				&& (FilterViewModel.CashDocumentType == null || FilterViewModel.CashDocumentType == CashDocumentType.AdvanceReport)
				&& (FilterViewModel.RestrictDocument is null || FilterViewModel.RestrictDocument == typeof(AdvanceReport)))
			{
				if(FilterViewModel.Subdivision is null)
				{
					query.WhereRestrictionOn(x => x.RelatedToSubdivision.Id)
						.IsIn(AvailableSubdivisionsIds);
				}
				else
				{
					query.Where(x => x.RelatedToSubdivision.Id == FilterViewModel.Subdivision.Id);
				}

				if(FilterViewModel.FinancialExpenseCategory != null)
				{
					query.Where(advanceReport => advanceReport.ExpenseCategoryId == FilterViewModel.FinancialExpenseCategory.Id);
				}

				if(FilterViewModel.StartDate != null)
				{
					query.Where(advanceReport => advanceReport.Date >= FilterViewModel.StartDate.Value);
				}

				if(FilterViewModel.EndDate != null)
				{
					query.Where(advanceReport => advanceReport.Date <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.Employee != null)
				{
					query.Where(advanceReport => advanceReport.Accountable == FilterViewModel.Employee);
				}

				query.Where(GetSearchCriterion(
					() => advanceReportAlias.Id,
					() => advanceReportAlias.Description,
					() => employeeAlias.Name,
					() => employeeAlias.LastName,
					() => employeeAlias.Patronymic,
					() => advanceReportAlias.Date,
					() => casherAlias.Name,
					() => casherAlias.LastName,
					() => casherAlias.Patronymic,
					() => advanceReportAlias.Money,
					() => financialExpenseCategoryAlias.Title));
			}
			else
			{
				query.Where(() => advanceReportAlias.Id == -1);
			}

			query.Left.JoinAlias(() => advanceReportAlias.Accountable, () => employeeAlias)
				.Left.JoinAlias(() => advanceReportAlias.Casher, () => casherAlias)
				.JoinEntityAlias(
					() => financialExpenseCategoryAlias,
					() => advanceReportAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(!_hasAccessToHiddenFinancialCategories)
			{
				query.Where(() =>
					(advanceReportAlias.ExpenseCategoryId == null
					|| (advanceReportAlias.ExpenseCategoryId != null && !financialExpenseCategoryAlias.IsHiddenFromPublicAccess)
					));
			}
			
			query.SelectList(list => list
					.Select(() => advanceReportAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(AdvanceReportProjections.GetTitleProjection()).WithAlias(() => resultAlias.Name)
					.Select(() => typeof(AdvanceReport)).WithAlias(() => resultAlias.EntityType)
					.Select(() => CashDocumentType.AdvanceReport).WithAlias(() => resultAlias.CashDocumentType)
					.Select(() => advanceReportAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(() => advanceReportAlias.Money).WithAlias(() => resultAlias.Money)
					.Select(() => advanceReportAlias.Description).WithAlias(() => resultAlias.Description)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => casherAlias.Name).WithAlias(() => resultAlias.CasherName)
					.Select(() => casherAlias.LastName).WithAlias(() => resultAlias.CasherSurname)
					.Select(() => casherAlias.Patronymic).WithAlias(() => resultAlias.CasherPatronymic)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.Category));

			return query
				.OrderBy(x => x.Date).Desc
				.TransformUsing(Transformers.AliasToBean<DocumentNode>());
		}
	}
}
