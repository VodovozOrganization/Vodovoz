using Autofac;
using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Utilities;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.NotificationSenders;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Cash;
using static Vodovoz.ViewModels.Journals.FilterViewModels.PayoutRequestJournalFilterViewModel;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class PayoutRequestsJournalViewModel : FilterableMultipleEntityJournalViewModelBase
		<PayoutRequestJournalNode, PayoutRequestJournalFilterViewModel>
	{
		private readonly IDictionary<Type, IPermissionResult> _domainObjectsPermissions;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly ICashRequestForDriverIsGivenForTakeNotificationSender _cashRequestForDriverIsGivenForTakeNotificationSender;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private readonly IGenericRepository<FinancialResponsibilityCenter> _financialResponsibilityCenterRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICashRepository _cashRepository;
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _scope;
		private readonly bool _createSelectAction;

		private bool _isAdmin;
		private bool _cashRequestFinancier;
		private bool _cashRequestCoordinator;
		private bool _roleCashier;
		private bool _roleSecurityService;
		private bool _canSeeCurrentSubdivisonRequests;
		private int _currentEmployeeId;
		private Employee _currentEmployee;
		private string _footerInfo;
		private bool _hasAccessToHiddenFinancialCategories;
		private IEnumerable<int> _subdivisionsControlledByCurrentEmployee;

		public PayoutRequestsJournalViewModel(
			PayoutRequestJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			ICashRepository cashRepository,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			ICurrentPermissionService currentPermissionService,
			ICashRequestForDriverIsGivenForTakeNotificationSender cashRequestForDriverIsGivenForTakeNotificationSender,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory, 
			IGenericRepository<FinancialResponsibilityCenter> financialResponsibilityCenterRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			bool createSelectAction = true)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_cashRequestForDriverIsGivenForTakeNotificationSender = cashRequestForDriverIsGivenForTakeNotificationSender
				?? throw new ArgumentNullException(nameof(cashRequestForDriverIsGivenForTakeNotificationSender));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory ?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			_financialResponsibilityCenterRepository = financialResponsibilityCenterRepository ?? throw new ArgumentNullException(nameof(financialResponsibilityCenterRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_createSelectAction = createSelectAction;

			TabName = "Журнал заявок ДС";

			UpdateOnChanges(
				typeof(CashRequest),
				typeof(CashlessRequest),
				typeof(CashRequestSumItem),
				typeof(Subdivision),
				typeof(Employee)
			);

			DomainObjectsTypes = new Type[]
			{
				typeof(CashRequest),
				typeof(CashlessRequest)
			};

			_domainObjectsPermissions = InitializePermissionsMatrix(DomainObjectsTypes);

			RegisterCashRequest();
			RegisterCashlessRequest();

			var threadLoader = DataLoader as ThreadDataLoader<PayoutRequestJournalNode>;
			threadLoader?.MergeInOrderBy(x => x.Date, @descending: true);
			DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;

			FinishJournalConfiguration();
			AccessRequest();

			_subdivisionsControlledByCurrentEmployee = GetSubdivisionsControlledByCurrentEmployee(UoW);

			FilterViewModel.IncludedAccountableSubdivision = _subdivisionsControlledByCurrentEmployee.ToArray();
			FilterViewModel.JournalViewModel = this;
			FilterViewModel.PropertyChanged += UpdateDataLoader;
		}

		private void UpdateDataLoader(object s, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(FilterViewModel.DocumentsSortOrder))
			{
				var threadLoader = DataLoader as ThreadDataLoader<PayoutRequestJournalNode>;
				threadLoader.OrderRules.Clear();

				if(FilterViewModel.DocumentsSortOrder == PayoutDocumentsSortOrder.ByCreationDate)
				{
					threadLoader?.MergeInOrderBy(x => x.Date, @descending: true);
					return;
				}

				if(FilterViewModel.DocumentsSortOrder == PayoutDocumentsSortOrder.ByMoneyTransferDate)
				{
					threadLoader?.MergeInOrderBy(x => x.MoneyTransferDate, @descending: true);
					threadLoader?.MergeInOrderBy(x => x.Date, @descending: true);
					return;
				}

				throw new NotSupportedException("Сортировка по выбранному параметру не поддерживается");
			}
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

		public Type[] DomainObjectsTypes { get; }

		public override string FooterInfo
		{
			get => _footerInfo;
			set => SetField(ref _footerInfo, value);
		}

		public ILifetimeScope Scope => _scope;

		private void OnDataLoaderItemsListUpdated(object sender, EventArgs e)
		{
			var totalSum = Items.Count > 0 ? Items.OfType<PayoutRequestJournalNode>().Sum(x => x.Sum) : 0;
			FooterInfo = $"{base.FooterInfo} | Сумма загруженных заявок: {totalSum.ToShortCurrencyString()}";
		}

		private void AccessRequest()
		{
			var userId = _commonServices.UserService.CurrentUserId;
			_currentEmployee = _employeeRepository.GetEmployeesForUser(UoW, userId).First();
			_currentEmployeeId = _currentEmployee.Id;

			_isAdmin = _commonServices.UserService.GetCurrentUser().IsAdmin;
			_cashRequestFinancier =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_financier_cash_request", userId);
			_cashRequestCoordinator =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_coordinator_cash_request", userId);
			_roleCashier = _commonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier, userId);
			_roleSecurityService =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_security_service_cash_request", userId);
			_canSeeCurrentSubdivisonRequests =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_see_current_subdivision_cash_requests");
			_hasAccessToHiddenFinancialCategories =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.FinancialCategory.HasAccessToHiddenFinancialCategories);
		}

		private IEnumerable<int> GetSubdivisionsControlledByCurrentEmployee(IUnitOfWork uow)
		{
			var financialResponsibilityCenterIds = _financialResponsibilityCenterRepository
				.GetValue(uow, frc => frc.Id, frc => frc.ResponsibleEmployeeId == _currentEmployee.Id)
				.Concat(_financialResponsibilityCenterRepository
					.GetValue(uow, frc => frc.Id, frc => frc.ViceResponsibleEmployeeId == _currentEmployee.Id))
				.Distinct()
				.ToArray();

			return _employeeRepository
				.GetControlledByEmployeeSubdivisionIds(uow, _currentEmployee.Id)
				.Concat(_subdivisionRepository.GetValue(
					uow,
					s => s.Id,
					s => financialResponsibilityCenterIds.Contains(s.Id)));
		}

		#region JournalActions

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			if(_createSelectAction)
			{
				CreateDefaultSelectAction();
			}
			CreateAddActions();
			CreateApproveAction();
			CreateSendForIssue();
			CreateEditAction();
			CreateDeleteAction();
		}

		private void CreateAddActions()
		{
			var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });

			foreach(var documentType in DomainObjectsTypes)
			{
				var incomeCreateNodeAction = new JournalAction(
				documentType.GetClassUserFriendlyName().Accusative.CapitalizeSentence(),
				(selected) => _domainObjectsPermissions[documentType].CanCreate,
				(selected) => _domainObjectsPermissions[documentType].CanCreate,
				(selected) =>
				{
					if(documentType == typeof(CashRequest))
					{
						NavigationManager.OpenViewModel<CashRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					}

					if(documentType == typeof(CashlessRequest))
					{
						NavigationManager.OpenViewModel<CashlessRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
					}
				});

				addParentNodeAction.ChildActionsList.Add(incomeCreateNodeAction);
			}

			NodeActionsList.Add(addParentNodeAction);
		}

		private void CreateSendForIssue()
		{
			var sendAction = new JournalAction("Отправить на выдачу",
				selected =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToArray();
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.Agreed))
					{
						return false;
					}

					return selectedNodes.All(
						selectedNode => EntityConfigs.ContainsKey(selectedNode.EntityType)
										&& EntityConfigs[selectedNode.EntityType].PermissionResult.CanUpdate);
				},
				selected => _cashRequestFinancier,
				selected =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToArray();
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.Agreed))
					{
						return;
					}

					foreach(var selectedNode in selectedNodes)
					{
						if(selectedNode.EntityType == typeof(CashRequest))
						{
							var cashRequestVM = CreateCashRequestViewModelForMassOpenWithoutGui(selectedNode);
							if(cashRequestVM.CanConveyForResults)
							{
								cashRequestVM.ConveyForResultsCommand.Execute();
							}
							cashRequestVM.Dispose();
						}
						else if(selectedNode.EntityType == typeof(CashlessRequest))
						{
							var cashlessRequestVM = CreateCashlessRequestViewModelForMassOpenWithoutGui(selectedNode);
							if(cashlessRequestVM.CanConveyForPayout)
							{
								cashlessRequestVM.ConveyForPayoutCommand.Execute();
							}
							cashlessRequestVM.Dispose();
						}
					}
				}
			);
			NodeActionsList.Add(sendAction);
		}

		private void CreateApproveAction()
		{
			var approveAction = new JournalAction("Согласовать",
				selected =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToArray();
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.AgreedBySubdivisionChief))
					{
						return false;
					}

					return selectedNodes.All(
						selectedNode => EntityConfigs.ContainsKey(selectedNode.EntityType)
										&& EntityConfigs[selectedNode.EntityType].PermissionResult.CanUpdate);
				},
				selected => _cashRequestCoordinator,
				selected =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToArray();
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.AgreedBySubdivisionChief))
					{
						return;
					}

					foreach(var selectedNode in selectedNodes)
					{
						if(selectedNode.EntityType == typeof(CashRequest))
						{
							var cashRequestVM = CreateCashRequestViewModelForMassOpenWithoutGui(selectedNode);
							cashRequestVM.ApproveCommand.Execute();
							cashRequestVM.Dispose();
						}
						else if(selectedNode.EntityType == typeof(CashlessRequest))
						{
							var cashlessRequestVM = CreateCashlessRequestViewModelForMassOpenWithoutGui(selectedNode);
							cashlessRequestVM.ApproveCommand.Execute();
							cashlessRequestVM.Dispose();
						}
					}
				}
			);
			NodeActionsList.Add(approveAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					PayoutRequestJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					PayoutRequestJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));
					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
				}
			);
			if(SelectionMode == JournalSelectionMode.None || !_createSelectAction)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToList();

					if(selectedNodes.Count != 1)
					{
						return false;
					}

					PayoutRequestJournalNode selectedNode = selectedNodes.First();

					if(selectedNode.PayoutRequestState != PayoutRequestState.New)
					{
						return false;
					}

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PayoutRequestJournalNode>().ToList();
					if(selectedNodes.Count != 1)
					{
						return;
					}

					PayoutRequestJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		#endregion

		#region Cash

		private void RegisterCashRequest()
		{
			var cashConfig = RegisterEntity(GetCashRequestQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<CashRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate())?.ViewModel,
					//функция диалога открытия документа
					(node) => NavigationManager.OpenViewModel<CashRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id))?.ViewModel,
					//функция идентификации документа
					node => node.EntityType == typeof(CashRequest),
					"Заявка на выдачу наличных Д/С",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			cashConfig.FinishConfiguration();
		}

		private CashRequestViewModel CreateCashRequestViewModelForMassOpenWithoutGui(PayoutRequestJournalNode node)
		{
			return new CashRequestViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				_unitOfWorkFactory,
				_commonServices,
				_employeeRepository,
				_cashRepository,
				NavigationManager,
				_cashRequestForDriverIsGivenForTakeNotificationSender,
				_scope.Resolve<ViewModelEEVMBuilder<Employee>>(),
				_scope.Resolve<ViewModelEEVMBuilder<Subdivision>>(),
				_scope.Resolve<ViewModelEEVMBuilder<FinancialExpenseCategory>>());
		}

		private IQueryOver<CashRequest> GetCashRequestQuery(IUnitOfWork uow)
		{
			CashRequest cashRequestAlias = null;
			Employee authorAlias = null;
			CashRequestSumItem cashRequestSumItemAlias = null;
			Employee accountableEmployeeAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;
			Subdivision subdivisionAlias = null;

			PayoutRequestJournalNode<CashRequest> resultAlias = null;

			var result = uow.Session.QueryOver(() => cashRequestAlias)
				.Left.JoinAlias(с => с.Sums, () => cashRequestSumItemAlias)
				.Left.JoinAlias(с => с.Author, () => authorAlias)
				.Left.JoinAlias(() => cashRequestSumItemAlias.AccountableEmployee, () => accountableEmployeeAlias)
				.Left.JoinAlias(() => accountableEmployeeAlias.Subdivision, () => subdivisionAlias)
				.JoinEntityAlias(() => financialExpenseCategoryAlias, () => financialExpenseCategoryAlias.Id == cashRequestAlias.ExpenseCategoryId, JoinType.LeftOuterJoin);

			if(FilterViewModel != null)
			{
				var startDate = FilterViewModel.StartDate;
				var endDate = FilterViewModel.EndDate;
				
				if(startDate.HasValue)
				{
					result.Where(() => cashRequestAlias.Date >= startDate.Value.Date);
				}

				if(endDate.HasValue)
				{
					result.Where(() => cashRequestAlias.Date < endDate.Value.Date.AddDays(1));
				}

				if(FilterViewModel.Author != null)
				{
					result.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.AccountableEmployee != null)
				{
					result.Where(() => accountableEmployeeAlias.Id == FilterViewModel.AccountableEmployee.Id);
				}

				if(FilterViewModel.State != null)
				{
					result.Where(() => cashRequestAlias.PayoutRequestState == FilterViewModel.State);
				}

				if(FilterViewModel.DocumentType != PayoutRequestDocumentType.CashRequest
				   && FilterViewModel.DocumentType != null
				   || FilterViewModel.Counterparty != null)
				{
					result.Where(cr => cr.Id == -1);
				}

				if(FilterViewModel.AccountableSubdivision != null)
				{
					result.Where(() => accountableEmployeeAlias.Subdivision.Id ==  FilterViewModel.AccountableSubdivision.Id);
				}
			}

			if(!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier && !_roleSecurityService)
			{
				if(_canSeeCurrentSubdivisonRequests)
				{
					if(_subdivisionsControlledByCurrentEmployee.Count() > 0)
					{
						result.Where(Restrictions.Disjunction()
							.Add(Restrictions.In(Projections.Property(() => authorAlias.Subdivision.Id), _subdivisionsControlledByCurrentEmployee.ToArray()))
							.Add(Restrictions.Eq(Projections.Property(() => authorAlias.Subdivision.Id), _currentEmployee.Subdivision.Id)));
					}
					else
					{
						result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
					}
				}
				else
				{
					result.Where(() => cashRequestAlias.Author.Id == _currentEmployeeId);
				}
			}

			if(!_hasAccessToHiddenFinancialCategories)
			{
				result.Where(() =>
					cashRequestAlias.ExpenseCategoryId == null
					|| (cashRequestAlias.ExpenseCategoryId != null && !financialExpenseCategoryAlias.IsHiddenFromPublicAccess)
					);
			}

			var authorProjection = CustomProjections.Concat_WS(" ",
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var accauntableProjection = CustomProjections.Concat_WS(" ",
				Projections.Property(() => accountableEmployeeAlias.LastName),
				Projections.Property(() => accountableEmployeeAlias.Name),
				Projections.Property(() => accountableEmployeeAlias.Patronymic)
			);

			var cashReuestSumSubquery = QueryOver.Of(() => cashRequestSumItemAlias)
				.Where(() => cashRequestSumItemAlias.CashRequest.Id == cashRequestAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<CashRequestSumItem>(o => o.CashRequest)))
				.Select(Projections.Sum(() => cashRequestSumItemAlias.Sum));

			Expense cashRequestSumItemExpenseAlias = null;

			var cashReuestSumGivedSubquery = QueryOver.Of(() => cashRequestSumItemAlias)
				.JoinAlias(() => cashRequestSumItemAlias.Expenses, () => cashRequestSumItemExpenseAlias)
				.Where(() => cashRequestSumItemAlias.CashRequest.Id == cashRequestAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<CashRequestSumItem>(o => o.CashRequest)))
				.Select(Projections.Sum(() => cashRequestSumItemExpenseAlias.Money));

			var moneyTransferDateSubquery = QueryOver.Of(() => cashRequestSumItemAlias)
				.Where(() => cashRequestSumItemAlias.CashRequest.Id == cashRequestAlias.Id)
				.Select(Projections.Max(() => cashRequestSumItemAlias.Date.Date));

			result.Where(GetSearchCriterion(
				() => cashRequestAlias.Id,
				() => authorAlias.Id,
				() => authorProjection,
				() => accauntableProjection,
				() => accountableEmployeeAlias.Id,
				() => cashRequestAlias.Basis,
				() => financialExpenseCategoryAlias.Title
			));

			result.SelectList(list => list
					.SelectGroup(c => c.Id)
					.Select(c => c.Id).WithAlias(() => resultAlias.Id)
					.Select(c => c.Date).WithAlias(() => resultAlias.Date)
					.Select(c => c.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
					.Select(c => c.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
					.Select(authorProjection).WithAlias(() => resultAlias.AuthorFullName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(accauntableProjection).WithAlias(() => resultAlias.AccountablePersonFullName)
					.Select(() => accountableEmployeeAlias.Name).WithAlias(() => resultAlias.AccountablePersonName)
					.Select(() => accountableEmployeeAlias.LastName).WithAlias(() => resultAlias.AccountablePersonLastName)
					.Select(() => accountableEmployeeAlias.Patronymic).WithAlias(() => resultAlias.AccountablePersonPatronymic)
					.SelectSubQuery(cashReuestSumSubquery).WithAlias(() => resultAlias.Sum)
					.SelectSubQuery(cashReuestSumGivedSubquery).WithAlias(() => resultAlias.SumGived)
					.SelectSubQuery(moneyTransferDateSubquery).WithAlias(() => resultAlias.MoneyTransferDate)
					.Select(c => c.Basis).WithAlias(() => resultAlias.Basis)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ExpenseCategory)
					.Select(c => c.HaveReceipt).WithAlias(() => resultAlias.HaveReceipt)
					.SelectSubQuery(moneyTransferDateSubquery).WithAlias(() => resultAlias.MoneyTransferDate)
				).TransformUsing(Transformers.AliasToBean<PayoutRequestJournalNode<CashRequest>>());

			if(FilterViewModel.DocumentsSortOrder == PayoutDocumentsSortOrder.ByMoneyTransferDate)
			{
				result.OrderBy(Projections.Property(nameof(PayoutRequestJournalNode.MoneyTransferDate))).Desc()
					.ThenBy(Projections.Property(nameof(PayoutRequestJournalNode.Id))).Desc();
			}
			else
			{
				result.OrderBy(x => x.Date).Desc();
			}

			return result;
		}

		#endregion

		#region Cashless

		private void RegisterCashlessRequest()
		{
			var cashlessConfig = RegisterEntity(GetCashlessRequestQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<CashlessRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(node) => NavigationManager.OpenViewModel<CashlessRequestViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id))?.ViewModel,
					//функция идентификации документа
					node => node.EntityType == typeof(CashlessRequest),
					"Заявка на оплату по Б/Н",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			cashlessConfig.FinishConfiguration();
		}

		private CashlessRequestViewModel CreateCashlessRequestViewModelForMassOpenWithoutGui(PayoutRequestJournalNode node)
		{
			return _scope.Resolve<CashlessRequestViewModel>(
				new TypedParameter(typeof(IEntityUoWBuilder),
				EntityUoWBuilder.ForOpen(node.Id))
			);
		}

		private IQueryOver<CashlessRequest> GetCashlessRequestQuery(IUnitOfWork uow)
		{
			CashlessRequest cashlessRequestAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			PayoutRequestJournalNode<CashlessRequest> resultAlias = null;

			var result = uow.Session.QueryOver(() => cashlessRequestAlias)
				.Left.JoinAlias(с => с.Author, () => authorAlias)
				.Left.JoinAlias(c => c.Counterparty, () => counterpartyAlias)
				.JoinEntityAlias(() => financialExpenseCategoryAlias, () => financialExpenseCategoryAlias.Id == cashlessRequestAlias.ExpenseCategoryId, JoinType.LeftOuterJoin);

			if(FilterViewModel != null)
			{
				if(FilterViewModel.StartDate.HasValue)
				{
					result.Where(() => cashlessRequestAlias.Date >= FilterViewModel.StartDate.Value.Date);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					result.Where(() => cashlessRequestAlias.Date < FilterViewModel.EndDate.Value.Date.AddDays(1));
				}

				if(FilterViewModel.Author != null)
				{
					result.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.State != null)
				{
					result.Where(() => cashlessRequestAlias.PayoutRequestState == FilterViewModel.State);
				}

				if(FilterViewModel.DocumentType != PayoutRequestDocumentType.CashlessRequest
				   && FilterViewModel.DocumentType != null
				   || FilterViewModel.AccountableEmployee != null
				   || FilterViewModel.AccountableSubdivision != null)
				{
					result.Where(clr => clr.Id == -1);
				}

				if(FilterViewModel.Counterparty != null)
				{
					result.Where(() => cashlessRequestAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}

				if(FilterViewModel.StartPaymentDatePlanned.HasValue)
				{
					result.Where(() => cashlessRequestAlias.PaymentDatePlanned >= FilterViewModel.StartPaymentDatePlanned.Value.Date);
				}

				if(FilterViewModel.EndPaymentDatePlanned.HasValue)
				{
					result.Where(() => cashlessRequestAlias.PaymentDatePlanned < FilterViewModel.EndPaymentDatePlanned.Value.Date.AddDays(1));
				}
			}

			if(!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier && !_roleSecurityService)
			{
				if(_canSeeCurrentSubdivisonRequests)
				{
					if(_subdivisionsControlledByCurrentEmployee.Count() > 0)
					{
						result.Where(Restrictions.In(Projections.Property(() => authorAlias.Subdivision.Id), _subdivisionsControlledByCurrentEmployee.ToArray()));
					}
					else
					{
						result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
					}
				}
				else
				{
					result.Where(() => cashlessRequestAlias.Author.Id == _currentEmployeeId);
				}
			}

			if(!_hasAccessToHiddenFinancialCategories)
			{
				result.Where(() =>
					cashlessRequestAlias.ExpenseCategoryId == null
					|| (cashlessRequestAlias.ExpenseCategoryId != null && !financialExpenseCategoryAlias.IsHiddenFromPublicAccess)
					);
			}

			var authorProjection = CustomProjections.Concat_WS(" ",
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			result.Where(GetSearchCriterion(
				() => cashlessRequestAlias.Id,
				() => authorAlias.Id,
				() => authorProjection,
				() => cashlessRequestAlias.Basis,
				() => financialExpenseCategoryAlias.Title
			));

			result.SelectList(list => list
					.SelectGroup(clr => clr.Id)
					.Select(clr => clr.Id).WithAlias(() => resultAlias.Id)
					.Select(clr => clr.Date).WithAlias(() => resultAlias.Date)
					.Select(clr => clr.PaymentDatePlanned).WithAlias(() => resultAlias.PaymentDatePlanned)
					.Select(clr => clr.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
					.Select(clr => clr.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
					.Select(authorProjection).WithAlias(() => resultAlias.AuthorFullName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(clr => clr.Basis).WithAlias(() => resultAlias.Basis)
					.Select(clr => clr.Sum).WithAlias(() => resultAlias.Sum)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ExpenseCategory)
					.Select(clr => clr.Date.Date).WithAlias(() => resultAlias.MoneyTransferDate)
					.Select(clr => clr.IsImidiatelyBill).WithAlias(() => resultAlias.IsImidiatelyBill)
				)
				.TransformUsing(Transformers.AliasToBean<PayoutRequestJournalNode<CashlessRequest>>());

			if(FilterViewModel.DocumentsSortOrder == PayoutDocumentsSortOrder.ByMoneyTransferDate)
			{
				result.OrderBy(Projections.Property(nameof(PayoutRequestJournalNode.MoneyTransferDate))).Desc()
					.ThenBy(Projections.Property(nameof(PayoutRequestJournalNode.Id))).Desc();
			}
			else
			{
				result.OrderBy(x => x.Date).Desc();
			}

			return result;
		}

		#endregion
	}
}
