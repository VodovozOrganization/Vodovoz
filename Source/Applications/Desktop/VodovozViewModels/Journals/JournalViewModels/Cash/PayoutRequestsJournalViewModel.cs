using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Utilities;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Cash
{
	public class PayoutRequestsJournalViewModel : FilterableMultipleEntityJournalViewModelBase
		<PayoutRequestJournalNode, PayoutRequestJournalFilterViewModel>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly ICommonServices _commonServices;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IExpenseCategorySelectorFactory _expenseCategorySelectorFactory;
		private readonly IFileDialogService _fileDialogService;
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

		public PayoutRequestsJournalViewModel(
			PayoutRequestJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			ICashRepository cashRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory,
			IFileDialogService fileDialogService,
			ILifetimeScope scope,
			bool createSelectAction = true) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_expenseCategorySelectorFactory = expenseCategorySelectorFactory
											  ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_scope = scope;
			_createSelectAction = createSelectAction;

			TabName = "Журнал заявок ДС";

			UpdateOnChanges(
				typeof(CashRequest),
				typeof(CashlessRequest),
				typeof(CashRequestSumItem),
				typeof(Subdivision),
				typeof(Employee)
			);

			RegisterCashRequest();
			RegisterCashlessRequest();

			var threadLoader = DataLoader as ThreadDataLoader<PayoutRequestJournalNode>;
			threadLoader?.MergeInOrderBy(x => x.Date, @descending: true);
			DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;

			FinishJournalConfiguration();
			AccessRequest();
		}

		public override string FooterInfo
		{
			get => _footerInfo;
			set => SetField(ref _footerInfo, value);
		}

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

			_isAdmin = _commonServices.UserService.GetCurrentUser(UoW).IsAdmin;
			_cashRequestFinancier =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_financier_cash_request", userId);
			_cashRequestCoordinator =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_coordinator_cash_request", userId);
			_roleCashier = _commonServices.PermissionService.ValidateUserPresetPermission("role_сashier", userId);
			_roleSecurityService =
				_commonServices.PermissionService.ValidateUserPresetPermission("role_security_service_cash_request", userId);
			_canSeeCurrentSubdivisonRequests =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_see_current_subdivision_cash_requests");
		}

		#region JournalActions

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			if(_createSelectAction)
			{
				CreateDefaultSelectAction();
			}
			CreateDefaultAddActions();
			CreateApproveAction();
			CreateSendForIssue();
			CreateEditAction();
			CreateDeleteAction();
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
							var cashRequestVM = CreateCashRequestViewModelForOpen(selectedNode);
							if (cashRequestVM.CanConveyForResults)
							{
								cashRequestVM.ConveyForResultsCommand.Execute();
							}
							cashRequestVM.Dispose();
						}
						else if(selectedNode.EntityType == typeof(CashlessRequest))
						{
							var cashlessRequestVM = CreateCashlessRequestViewModelForOpen(selectedNode);
							if (cashlessRequestVM.CanConveyForPayout)
							{
								cashlessRequestVM.ConveyForPayout();
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
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.Submited))
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
					if(!selectedNodes.Any() || selectedNodes.Any(x => x.PayoutRequestState != PayoutRequestState.Submited))
					{
						return;
					}

					foreach(var selectedNode in selectedNodes)
					{
						if(selectedNode.EntityType == typeof(CashRequest))
						{
							var cashRequestVM = CreateCashRequestViewModelForOpen(selectedNode);
							cashRequestVM.ApproveCommand.Execute();
							cashRequestVM.Dispose();
						}
						else if(selectedNode.EntityType == typeof(CashlessRequest))
						{
							var cashlessRequestVM = CreateCashlessRequestViewModelForOpen(selectedNode);
							cashlessRequestVM.Approve();
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

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
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
					() => new CashRequestViewModel(
						EntityUoWBuilder.ForCreate(),
						_unitOfWorkFactory,
						_commonServices,
						_employeeRepository,
						_cashRepository,
						NavigationManager,
						_scope
					),
					//функция диалога открытия документа
					CreateCashRequestViewModelForOpen,
					//функция идентификации документа
					node => node.EntityType == typeof(CashRequest),
					"Заявка на выдачу наличных Д/С",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			cashConfig.FinishConfiguration();
		}

		private CashRequestViewModel CreateCashRequestViewModelForOpen(PayoutRequestJournalNode node)
		{
			return new CashRequestViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				_unitOfWorkFactory,
				_commonServices,
				_employeeRepository,
				_cashRepository,
				NavigationManager,
				_scope
			);
		}

		private IQueryOver<CashRequest> GetCashRequestQuery(IUnitOfWork uow)
		{
			CashRequest cashRequestAlias = null;
			Employee authorAlias = null;
			CashRequestSumItem cashRequestSumItemAlias = null;
			Employee accountableEmployeeAlias = null;

			PayoutRequestJournalNode<CashRequest> resultAlias = null;

			var result = uow.Session.QueryOver(() => cashRequestAlias)
				.Left.JoinAlias(с => с.Sums, () => cashRequestSumItemAlias)
				.Left.JoinAlias(с => с.Author, () => authorAlias)
				.Left.JoinAlias(() => cashRequestSumItemAlias.AccountableEmployee, () => accountableEmployeeAlias);

			if(FilterViewModel != null)
			{
				if(FilterViewModel.StartDate.HasValue)
				{
					result.Where(() => cashRequestAlias.Date >= FilterViewModel.StartDate.Value.Date);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					result.Where(() => cashRequestAlias.Date < FilterViewModel.EndDate.Value.Date.AddDays(1));
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
			}

			if(!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier && !_roleSecurityService)
			{
				if(_canSeeCurrentSubdivisonRequests)
				{
					result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
				}
				else
				{
					result.Where(() => cashRequestAlias.Author.Id == _currentEmployeeId);
				}
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

			result.Where(GetSearchCriterion(
				() => cashRequestAlias.Id,
				() => authorAlias.Id,
				() => authorProjection,
				() => accauntableProjection,
				() => accountableEmployeeAlias.Id,
				() => cashRequestAlias.Basis
			));

			result.SelectList(list => list
					.SelectGroup(c => c.Id)
					.Select(c => c.Id).WithAlias(() => resultAlias.Id)
					.Select(c => c.Date).WithAlias(() => resultAlias.Date)
					.Select(c => c.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
					.Select(c => c.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(accauntableProjection).WithAlias(() => resultAlias.AccountablePerson)
					.SelectSubQuery(cashReuestSumSubquery).WithAlias(() => resultAlias.Sum)
					.Select(c => c.Basis).WithAlias(() => resultAlias.Basis)
				).TransformUsing(Transformers.AliasToBean<PayoutRequestJournalNode<CashRequest>>())
				.OrderBy(x => x.Date).Desc();
			return result;
		}

		#endregion

		#region Cashless

		private void RegisterCashlessRequest()
		{
			var cashlessConfig = RegisterEntity(GetCashlessRequestQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new CashlessRequestViewModel(
						_fileDialogService,
						_expenseCategorySelectorFactory,
						new UserRepository(),
						_counterpartyJournalFactory,
						new EmployeeRepository(),
						EntityUoWBuilder.ForCreate(),
						_unitOfWorkFactory,
						_commonServices,
						NavigationManager,
						_scope
					),
					//функция диалога открытия документа
					CreateCashlessRequestViewModelForOpen,
					//функция идентификации документа
					node => node.EntityType == typeof(CashlessRequest),
					"Заявка на оплату по Б/Н",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = false }
				);

			//завершение конфигурации
			cashlessConfig.FinishConfiguration();
		}

		private CashlessRequestViewModel CreateCashlessRequestViewModelForOpen(PayoutRequestJournalNode node)
		{
			return new CashlessRequestViewModel(
				_fileDialogService,
				_expenseCategorySelectorFactory,
				new UserRepository(),
				_counterpartyJournalFactory,
				new EmployeeRepository(),
				EntityUoWBuilder.ForOpen(node.Id),
				_unitOfWorkFactory,
				_commonServices,
				NavigationManager,
				_scope
			);
		}

		private IQueryOver<CashlessRequest> GetCashlessRequestQuery(IUnitOfWork uow)
		{
			CashlessRequest cashlessRequestAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;

			PayoutRequestJournalNode<CashlessRequest> resultAlias = null;

			var result = uow.Session.QueryOver(() => cashlessRequestAlias)
				.Left.JoinAlias(с => с.Author, () => authorAlias)
				.Left.JoinAlias(c => c.Counterparty, () => counterpartyAlias);

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
				   || FilterViewModel.AccountableEmployee != null)
				{
					result.Where(clr => clr.Id == -1);
				}

				if(FilterViewModel.Counterparty != null)
				{
					result.Where(() => cashlessRequestAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}
			}

			if(!_isAdmin && !_cashRequestFinancier && !_cashRequestCoordinator && !_roleCashier && !_roleSecurityService)
			{
				if(_canSeeCurrentSubdivisonRequests)
				{
					result.Where(() => authorAlias.Subdivision.Id == _currentEmployee.Subdivision.Id);
				}
				else
				{
					result.Where(() => cashlessRequestAlias.Author.Id == _currentEmployeeId);
				}
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
				() => cashlessRequestAlias.Basis
			));

			result.SelectList(list => list
					.SelectGroup(clr => clr.Id)
					.Select(clr => clr.Id).WithAlias(() => resultAlias.Id)
					.Select(clr => clr.Date).WithAlias(() => resultAlias.Date)
					.Select(clr => clr.PayoutRequestState).WithAlias(() => resultAlias.PayoutRequestState)
					.Select(clr => clr.PayoutRequestDocumentType).WithAlias(() => resultAlias.PayoutRequestDocumentType)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(clr => clr.Basis).WithAlias(() => resultAlias.Basis)
					.Select(clr => clr.Sum).WithAlias(() => resultAlias.Sum)
				).TransformUsing(Transformers.AliasToBean<PayoutRequestJournalNode<CashlessRequest>>())
				.OrderBy(clr => clr.Date).Desc();
			return result;
		}

		#endregion
	}
}
