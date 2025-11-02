using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel : DialogViewModelBase
	{
		private const string _decimalFormatString = "# ##0.00";

		private static readonly OrderStatus[] _availableOrderStatuses = new OrderStatus[]
		{
			OrderStatus.Accepted,
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay,
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private readonly ICommonServices _commonServices;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<PaymentsDiscrepanciesAnalysisViewModel> _logger;

		private CounterpartySettlementsReconciliation1C _counterpartySettlementsReconciliation1C;
		private TurnoverBalanceSheet1C _turnoverBalanceSheet1C;

		private IDictionary<int, OrderDiscrepanciesNode> _orderDiscrepanciesNodes;
		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> _paymentDiscrepanciesNodes;
		private IDictionary<string, CounterpartyBalanceNode> _counterpartyBalanceNodes;

		private DiscrepancyCheckMode _selectedCheckMode;
		private string _selectedFileName;
		private Domain.Client.Counterparty _selectedClient;
		private bool _isDiscrepanciesOnly;
		private bool _isClosedOrdersOnly;
		private bool _isExcludeOldData;
		private DateTime? _commonReconciliationDataMaxDate;

		private string _ordersTotalSum1C;
		private string _paymentsTotalSum1C;
		private string _balance1C;
		private string _oldBalance1C;
		private string _ordersTotalSumInDatabase;
		private string _paymentsTotalSumInDatabase;
		private string _balanceInDatabase;
		private string _oldBalanceInDatabase;

		private OrderPaymentStatus? _orderPaymentStatus;
		private bool _hideUnregisteredCounterparties;

		public PaymentsDiscrepanciesAnalysisViewModel(
			INavigationManager navigation,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository,
			ILogger<PaymentsDiscrepanciesAnalysisViewModel> logger) : base(navigation)
		{
			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_interactiveService = _commonServices.InteractiveService;

			Title = "Поиск расхождений в оплатах клиента";

			SetByCounterpartyCheckModeCommand = new DelegateCommand(SetByCounterpartyCheckMode);
			SetCommonReconciliationCheckModeCommand = new DelegateCommand(SetCommonReconciliationCheckMode);
			AnalyseDiscrepanciesCommand = new DelegateCommand(AnalyseDiscrepancies, () => CanReadFile);

			OrdersNodes = new GenericObservableList<OrderDiscrepanciesNode>();
			OrderDiscrepancyDuplicateNodes = new GenericObservableList<OrderDiscrepanciesNode>();
			PaymentsNodes = new GenericObservableList<PaymentDiscrepanciesNode>();
			BalanceNodes = new GenericObservableList<CounterpartyBalanceNode>();
			Clients = new GenericObservableList<Domain.Client.Counterparty>();

			_isClosedOrdersOnly = true;
		}
		
		#region Settings

		public DiscrepancyCheckMode SelectedCheckMode
		{
			get => _selectedCheckMode;
			set => SetField(ref _selectedCheckMode, value);
		}

		public string SelectedFileName
		{
			get => _selectedFileName;
			set
			{
				if(SetField(ref _selectedFileName, value))
				{
					OnPropertyChanged(nameof(CanReadFile));
				}
			}
		}

		public Domain.Client.Counterparty SelectedClient
		{
			get => _selectedClient;
			set => SetField(ref _selectedClient, value);
		}

		public bool IsDiscrepanciesOnly
		{
			get => _isDiscrepanciesOnly;
			set
			{
				if(SetField(ref _isDiscrepanciesOnly, value))
				{
					FillOrderNodes();
					FillPaymentNodes();
				}
			}
		}

		public bool IsClosedOrdersOnly
		{
			get => _isClosedOrdersOnly;
			set
			{
				if(SetField(ref _isClosedOrdersOnly, value))
				{
					FillOrderNodes();
					FillPaymentNodes();
				}
			}
		}

		public bool IsExcludeOldData
		{
			get => _isExcludeOldData;
			set
			{
				if(SetField(ref _isExcludeOldData, value))
				{
					FillOrderNodes();
					FillPaymentNodes();
				}
			}
		}

		public DateTime? CommonReconciliationDataMaxDate
		{
			get => _commonReconciliationDataMaxDate;
			set
			{
				if(SetField(ref _commonReconciliationDataMaxDate, value))
				{
					if(_turnoverBalanceSheet1C != null)
					{
						AnalyseDiscrepanciesCommand?.Execute();
					}
				}
			}
		}

		public string OrdersTotalSum1C
		{
			get => _ordersTotalSum1C;
			set => SetField(ref _ordersTotalSum1C, value);
		}

		public string PaymentsTotalSum1C
		{
			get => _paymentsTotalSum1C;
			set => SetField(ref _paymentsTotalSum1C, value);
		}

		public string Balance1C
		{
			get => _balance1C;
			set => SetField(ref _balance1C, value);
		}

		public string OldBalance1C
		{
			get => _oldBalance1C;
			set => SetField(ref _oldBalance1C, value);
		}

		public string OrdersTotalSumInDatabase
		{
			get => _ordersTotalSumInDatabase;
			set => SetField(ref _ordersTotalSumInDatabase, value);
		}

		public string PaymentsTotalSumInDatabase
		{
			get => _paymentsTotalSumInDatabase;
			set => SetField(ref _paymentsTotalSumInDatabase, value);
		}

		public string BalanceInDatabase
		{
			get => _balanceInDatabase;
			set => SetField(ref _balanceInDatabase, value);
		}

		public string OldBalanceInDatabase
		{
			get => _oldBalanceInDatabase;
			set => SetField(ref _oldBalanceInDatabase, value);
		}

		public OrderPaymentStatus? OrderPaymentStatus
		{
			get => _orderPaymentStatus;
			set
			{
				if(SetField(ref _orderPaymentStatus, value))
				{
					FillOrderNodes();
				}
			}
		}

		public bool HideUnregisteredCounterparties
		{
			get => _hideUnregisteredCounterparties;
			set
			{
				if(SetField(ref _hideUnregisteredCounterparties, value))
				{
					AnalyseDiscrepanciesCommand?.Execute();
				}
			}
		}

		public bool CanReadFile => !string.IsNullOrWhiteSpace(_selectedFileName);

		public GenericObservableList<OrderDiscrepanciesNode> OrdersNodes { get; }
		public GenericObservableList<PaymentDiscrepanciesNode> PaymentsNodes { get; }
		public GenericObservableList<CounterpartyBalanceNode> BalanceNodes { get; }
		public GenericObservableList<Domain.Client.Counterparty> Clients { get; private set; }
		public GenericObservableList<OrderDiscrepanciesNode> OrderDiscrepancyDuplicateNodes { get; }

		#endregion

		#region Commands

		public DelegateCommand SetByCounterpartyCheckModeCommand { get; }
		public DelegateCommand SetCommonReconciliationCheckModeCommand { get; }
		public DelegateCommand AnalyseDiscrepanciesCommand { get; }

		#endregion

		private void SetByCounterpartyCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.ReconciliationByCounterparty;
		}

		private void SetCommonReconciliationCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.CommonReconciliation;
		}

		private void AnalyseDiscrepancies()
		{
			if(!CanReadFile)
			{
				return;
			}

			_counterpartySettlementsReconciliation1C = null;
			_turnoverBalanceSheet1C = null;

			if(SelectedCheckMode == DiscrepancyCheckMode.ReconciliationByCounterparty)
			{
				CreateCounterpartySettlementsReconciliation1CFromXlsx();
			}

			if(SelectedCheckMode == DiscrepancyCheckMode.CommonReconciliation)
			{
				CreateTurnoverBalanceSheet1CFromXlsx();
			}

			SetSelectedClient();
			UpdateOrderDiscrepanciesNodes();
			UpdatePaymentDiscrepanciesNodes();
			UpdateCounterpartyBalanceNodes();
			FillOrderNodes();
			FillPaymentNodes();
			FillCounterpartyBalanceNodes();
			UpdateCounterpartySummaryInfo();

			if(OrderDiscrepancyDuplicateNodes.Any())
			{
				var sb = new StringBuilder();

				foreach(var orderDiscrepancy in OrderDiscrepancyDuplicateNodes)
				{
					sb.AppendLine(orderDiscrepancy.OrderId.ToString());
				}
				
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Следующие заказы дублируются в документе, что не поддерживается текущей логикой:\n"
					+ sb
					+ "Обратитесь в отдел разработки");
			}
		}

		private void CreateCounterpartySettlementsReconciliation1CFromXlsx()
		{
			try
			{
				_logger.LogInformation("Начинаем парсинг файла сверки взаиморасчетов по контрагенту");

				_counterpartySettlementsReconciliation1C = CounterpartySettlementsReconciliation1C.CreateFromXlsx(SelectedFileName);

				_logger.LogInformation("Парсинг файла закончен успешно");
			}
			catch(Exception ex)
			{
				var errorMessage = $"При парсинге файла возникла ошибка";

				_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

				_logger.LogDebug($"{errorMessage}: {ex.Message}");
			}
		}

		private void CreateTurnoverBalanceSheet1CFromXlsx()
		{
			try
			{
				_logger.LogInformation("Начинаем парсинг файла оборотно-сальдовой ведомости");

				_turnoverBalanceSheet1C = TurnoverBalanceSheet1C.CreateFromXlsx(SelectedFileName);

				_logger.LogInformation("Парсинг файла закончен успешно");
			}
			catch(Exception ex)
			{
				var errorMessage = $"При парсинге файла возникла ошибка";

				_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

				_logger.LogDebug($"{errorMessage}: {ex.Message}");
			}
		}

		private void SetSelectedClient()
		{
			SelectedClient = null;
			Clients.Clear();

			if(_counterpartySettlementsReconciliation1C is null)
			{
				return;
			}

			if(string.IsNullOrEmpty(_counterpartySettlementsReconciliation1C.CounterpartyInn))
			{
				var message = $"В указанном файле ИНН контрагента не найдено";

				_logger.LogDebug(message);
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);

				return;
			}

			var counterparties = GetCounterpartiesByInn(_counterpartySettlementsReconciliation1C.CounterpartyInn);

			if(counterparties.Count != 1)
			{
				var message = $"Найдено {counterparties.Count} контрагентов с ИНН {_counterpartySettlementsReconciliation1C.CounterpartyInn}. Должно быть 1";

				_logger.LogDebug(message);
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);

				return;
			}

			Clients.Add(counterparties.First());

			OnPropertyChanged(nameof(Clients));

			SelectedClient = counterparties.First();
		}

		private IList<Domain.Client.Counterparty> GetCounterpartiesByInn(string counterpartyInn)
		{
			return _counterpartyRepository.GetCounterpartiesByINN(_unitOfWork, counterpartyInn);
		}

		#region UpdateOrderDiscrepanciesNodes
		private void UpdateOrderDiscrepanciesNodes()
		{
			_orderDiscrepanciesNodes = null;

			if(_counterpartySettlementsReconciliation1C is null || SelectedClient is null)
			{
				return;
			}

			var orders1C = _counterpartySettlementsReconciliation1C.Orders;

			var allocations =
				GetAllocationsFromDatabase(SelectedClient.Id, SelectedClient.INN, orders1C.Select(o => o.OrderId).ToList());

			_orderDiscrepanciesNodes = CreateOrderDiscrepanciesNodes(orders1C, allocations);
		}

		private IList<OrderWithAllocation> GetAllocationsFromDatabase(int clientId, string clientInn, IList<int> orderIds)
		{
			var allocations = _orderRepository.GetOrdersWithAllocationsOnDayByOrdersIds(_unitOfWork, orderIds);
			var ordersMissingFromDocumentAllocatedToClient = _orderRepository.GetOrdersWithAllocationsOnDayByCounterparty(_unitOfWork, clientId, orderIds);
			var ordersMissingFromDocumentAllocatedToAnotherClient =
				_orderRepository.GetAllocationsToOrdersWithAnotherClient(_unitOfWork, clientId, clientInn, orderIds);

			return allocations
				.Concat(ordersMissingFromDocumentAllocatedToClient)
				.Concat(ordersMissingFromDocumentAllocatedToAnotherClient)
				.ToList();
		}

		private IDictionary<int, OrderDiscrepanciesNode> CreateOrderDiscrepanciesNodes(
			IList<OrderReconciliation1C> orders1C,
			IList<OrderWithAllocation> allocations)
		{
			var orderDiscrepanciesNodes = CreateOrderDiscrepanciesNodesFromOrderReconciliation1C(orders1C);

			foreach(var allocation in allocations)
			{
				if(orderDiscrepanciesNodes.TryGetValue(allocation.OrderId, out var node))
				{
					node.OrderDeliveryDateInDatabase = allocation.OrderDeliveryDate;
					node.OrderStatus = allocation.OrderStatus;
					node.OrderPaymentStatus = allocation.OrderPaymentStatus;
					node.ProgramOrderSum = allocation.OrderSum;
					node.AllocatedSum = allocation.OrderAllocation;
					node.IsMissingFromDocument = allocation.IsMissingFromDocument;
					node.OrderClientNameInDatabase = allocation.OrderClientName;
					node.OrderClientInnInDatabase = allocation.OrderClientInn;
				}
				else
				{
					var orderDiscrepanciesNode = new OrderDiscrepanciesNode
					{
						OrderId = allocation.OrderId,
						OrderDeliveryDateInDatabase = allocation.OrderDeliveryDate,
						OrderStatus = allocation.OrderStatus,
						OrderPaymentStatus = allocation.OrderPaymentStatus,
						ProgramOrderSum = allocation.OrderSum,
						AllocatedSum = allocation.OrderAllocation,
						IsMissingFromDocument = allocation.IsMissingFromDocument,
						OrderClientNameInDatabase = allocation.OrderClientName,
						OrderClientInnInDatabase = allocation.OrderClientInn
					};

					orderDiscrepanciesNodes.Add(orderDiscrepanciesNode.OrderId, orderDiscrepanciesNode);
				}
			}

			return orderDiscrepanciesNodes;
		}

		private IDictionary<int, OrderDiscrepanciesNode> CreateOrderDiscrepanciesNodesFromOrderReconciliation1C(
			IList<OrderReconciliation1C> orders1C)
		{
			OrderDiscrepancyDuplicateNodes.Clear();
			var orderDiscrepanciesNodes = new Dictionary<int, OrderDiscrepanciesNode>();

			foreach(var orderReconciliation in orders1C)
			{
				var orderDiscrepanciesNode = new OrderDiscrepanciesNode
				{
					OrderId = orderReconciliation.OrderId,
					DocumentOrderSum = orderReconciliation.OrderSum,
					OrderDeliveryDateInDocument = orderReconciliation.OrderDeliveryDate
				};

				if(orderDiscrepanciesNodes.ContainsKey(orderDiscrepanciesNode.OrderId))
				{
					OrderDiscrepancyDuplicateNodes.Add(orderDiscrepanciesNode);
					continue;
				}
				
				orderDiscrepanciesNodes.Add(orderDiscrepanciesNode.OrderId, orderDiscrepanciesNode);
			}

			return orderDiscrepanciesNodes;
		}
		#endregion

		#region UpdatePaymentDiscrepanciesNodes

		private void UpdatePaymentDiscrepanciesNodes()
		{
			_paymentDiscrepanciesNodes = null;

			if(_counterpartySettlementsReconciliation1C is null || SelectedClient is null)
			{
				return;
			}

			var payments1C = _counterpartySettlementsReconciliation1C.Payments;

			var paymentFromDatabase = GetCounterpartyPaymentsFromDatabase(SelectedClient.INN, SelectedClient.Id);

			_paymentDiscrepanciesNodes = CreatePaymentDiscrepanciesNodes(payments1C, paymentFromDatabase);
		}

		private IList<PaymentNode> GetCounterpartyPaymentsFromDatabase(
			string counterpartyInn,
			int counterpartyId)
		{
			var payments = _paymentsRepository
				.GetCounterpartyPaymentNodes(_unitOfWork, counterpartyId, counterpartyInn)
				.ToList();

			return payments;
		}

		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> CreatePaymentDiscrepanciesNodes(
			IList<PaymentReconciliation1C> payments1C,
			IList<PaymentNode> paymentsFromDatabase)
		{
			var paymentDiscrepanciesNodes = CreatePaymentDiscrepanciesNodesFromReconciliations1C(payments1C);

			foreach(var paymentDatabase in paymentsFromDatabase)
			{
				if(paymentDiscrepanciesNodes.TryGetValue((paymentDatabase.PaymentNum, paymentDatabase.PaymentDate), out var paymentDiscrepanciesNode))
				{
					paymentDiscrepanciesNode.ProgramPaymentSum = paymentDatabase.PaymentSum;
					paymentDiscrepanciesNode.IsManuallyCreated = paymentDatabase.IsManuallyCreated;
					paymentDiscrepanciesNode.CounterpartyId = paymentDatabase.CounterpartyId;
					paymentDiscrepanciesNode.CounterpartyName = paymentDatabase.CounterpartyName;
					paymentDiscrepanciesNode.CounterpartyInn = paymentDatabase.CounterpartyInn;
					paymentDiscrepanciesNode.PaymentPurpose = paymentDatabase.PaymentPurpose;

					if(paymentDatabase.PayerName != paymentDatabase.CounterpartyName
						&& paymentDatabase.PayerName != paymentDatabase.CounterpartyFullName)
					{
						paymentDiscrepanciesNode.PayerName = paymentDatabase.PayerName;
					}
				}
				else
				{
					var newNode = new PaymentDiscrepanciesNode
					{
						PaymentNum = paymentDatabase.PaymentNum,
						PaymentDate = paymentDatabase.PaymentDate,
						ProgramPaymentSum = paymentDatabase.PaymentSum,
						IsManuallyCreated = paymentDatabase.IsManuallyCreated,
						CounterpartyId = paymentDatabase.CounterpartyId,
						CounterpartyName = paymentDatabase.CounterpartyName,
						CounterpartyInn = paymentDatabase.CounterpartyInn,
						PaymentPurpose = paymentDatabase.PaymentPurpose
					};

					if(paymentDatabase.PayerName != paymentDatabase.CounterpartyName
						&& paymentDatabase.PayerName != paymentDatabase.CounterpartyFullName)
					{
						newNode.PayerName = paymentDatabase.PayerName;
					}

					paymentDiscrepanciesNodes.Add((paymentDatabase.PaymentNum, paymentDatabase.PaymentDate), newNode);
				}
			}

			return paymentDiscrepanciesNodes;
		}

		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> CreatePaymentDiscrepanciesNodesFromReconciliations1C(
			IList<PaymentReconciliation1C> paymentReconciliations)
		{
			var paymentDiscrepanciesNodes = new Dictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode>();

			foreach(var paymentReconciliation in paymentReconciliations)
			{
				if(paymentDiscrepanciesNodes.TryGetValue((paymentReconciliation.PaymentNum, paymentReconciliation.PaymentDate), out var payment))
				{
					payment.DocumentPaymentSum += paymentReconciliation.PaymentSum;

					continue;
				}

				var paymentDiscrepanciesNode = new PaymentDiscrepanciesNode
				{
					PaymentNum = paymentReconciliation.PaymentNum,
					PaymentDate = paymentReconciliation.PaymentDate,
					DocumentPaymentSum = paymentReconciliation.PaymentSum
				};

				paymentDiscrepanciesNodes.Add((paymentDiscrepanciesNode.PaymentNum, paymentDiscrepanciesNode.PaymentDate), paymentDiscrepanciesNode);
			}

			return paymentDiscrepanciesNodes;
		}
		#endregion

		#region UpdateCounterpartyBalanceNodes
		private void UpdateCounterpartyBalanceNodes()
		{
			_counterpartyBalanceNodes = null;

			if(_turnoverBalanceSheet1C is null)
			{
				return;
			}

			if(_turnoverBalanceSheet1C.CounterpartyBalances.Count == 0)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"В указанном файле не найдены данные по балансу контрагентов.");

				return;
			}

			var balances1C = _turnoverBalanceSheet1C.CounterpartyBalances;
			var balancesFromDatabase = GetCounterpartyBalancesFromDatabase();

			_counterpartyBalanceNodes = CreateCounterpartyBalanceNodes(balances1C, balancesFromDatabase);

			FillEmptyNamesInCounterpartyBalanceNodes();
		}

		private IList<CounterpartyCashlessBalanceNode> GetCounterpartyBalancesFromDatabase()
		{
			if(CommonReconciliationDataMaxDate.HasValue)
			{
				return _counterpartyRepository
					.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, maxDeliveryDate: CommonReconciliationDataMaxDate.Value)
					.ToList();
			}

			return _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses)
				.ToList();
		}

		private IDictionary<string, CounterpartyBalanceNode> CreateCounterpartyBalanceNodes(
			IList<CounterpartyBalance1C> balances1C,
			IList<CounterpartyCashlessBalanceNode> balancesFromDatabase)
		{
			var counterpartyBalanceNodes = CreateCounterpartyBalanceNodesFromBalanceSheet1C(balances1C);

			foreach(var databaseBalanceNode in balancesFromDatabase)
			{
				if(counterpartyBalanceNodes.TryGetValue(databaseBalanceNode.CounterpartyInn, out var balance))
				{
					balance.CounterpartyInn = databaseBalanceNode.CounterpartyInn;
					balance.CounterpartyName = $"{databaseBalanceNode.CounterpartyName}";
					balance.CounterpartyBalance = databaseBalanceNode.Balance;
				}
				else
				{
					counterpartyBalanceNodes.Add(
						databaseBalanceNode.CounterpartyInn,
						new CounterpartyBalanceNode
						{
							CounterpartyInn = databaseBalanceNode.CounterpartyInn,
							CounterpartyName = $"{databaseBalanceNode.CounterpartyName}",
							CounterpartyBalance = databaseBalanceNode.Balance
						});
				}
			}

			return counterpartyBalanceNodes;
		}

		private IDictionary<string, CounterpartyBalanceNode> CreateCounterpartyBalanceNodesFromBalanceSheet1C(
			IList<CounterpartyBalance1C> balances1C)
		{
			var balanceNodes = new Dictionary<string, CounterpartyBalanceNode>();

			foreach(var balance in balances1C)
			{
				if(balanceNodes.TryGetValue(balance.Inn, out var node))
				{
					node.CounterpartyBalance1C += balance.Balance;

					continue;
				}

				var balanceNode = new CounterpartyBalanceNode
				{
					CounterpartyInn = balance.Inn,
					CounterpartyBalance1C = balance.Balance
				};

				balanceNodes.Add(balance.Inn, balanceNode);
			}

			return balanceNodes;
		}

		private void FillEmptyNamesInCounterpartyBalanceNodes()
		{
			var emptyNameInns = _counterpartyBalanceNodes
				.Where(b => string.IsNullOrWhiteSpace(b.Value.CounterpartyName))
				.Select(b => b.Key)
				.Distinct()
				.ToList();

			var counterpartyNames = GetCounterpartyNamesByInn(emptyNameInns);

			foreach(var inn in emptyNameInns)
			{
				if(counterpartyNames.TryGetValue(inn, out var name))
				{
					_counterpartyBalanceNodes[inn].CounterpartyName = $"{name}";
				}
			}
		}

		private Dictionary<string, string> GetCounterpartyNamesByInn(IList<string> counterpartyInns)
		{
			var counterpartyInnName = _counterpartyRepository
				.GetCounterpartyNamesByInn(_unitOfWork, counterpartyInns)
				.DistinctBy(c => c.Inn)
				.ToDictionary(c => c.Inn, c => c.Name);

			return counterpartyInnName;
		}
		#endregion

		private void FillOrderNodes()
		{
			OrdersNodes.Clear();

			if(_orderDiscrepanciesNodes is null)
			{
				return;
			}

			foreach(var keyPairValue in _orderDiscrepanciesNodes)
			{
				var orderNode = keyPairValue.Value;

				if(OrderPaymentStatus != null && orderNode.OrderPaymentStatus != OrderPaymentStatus)
				{
					continue;
				}

				if(IsDiscrepanciesOnly && !orderNode.OrderSumDiscrepancy)
				{
					continue;
				}

				if(IsClosedOrdersOnly && orderNode.OrderStatus != OrderStatus.Closed)
				{
					continue;
				}

				if(IsExcludeOldData
					&& (orderNode.OrderDeliveryDateInDatabase < CounterpartySettlementsReconciliation1C.OldOrdersMaxDate.AddDays(1))
						|| (!orderNode.OrderDeliveryDateInDatabase.HasValue && orderNode.OrderDeliveryDate < CounterpartySettlementsReconciliation1C.OldOrdersMaxDate.AddDays(1)))
				{
					continue;
				}

				OrdersNodes.Add(orderNode);
			}
		}

		private void FillPaymentNodes()
		{
			PaymentsNodes.Clear();

			if(_paymentDiscrepanciesNodes is null)
			{
				return;
			}

			foreach(var keyPairValue in _paymentDiscrepanciesNodes)
			{
				var paymentNode = keyPairValue.Value;

				if(IsDiscrepanciesOnly && paymentNode.DocumentPaymentSum == paymentNode.ProgramPaymentSum)
				{
					continue;
				}

				if(IsExcludeOldData
					&& paymentNode.PaymentDate < CounterpartySettlementsReconciliation1C.OldOrdersMaxDate.AddDays(1))
				{
					continue;
				}

				PaymentsNodes.Add(paymentNode);
			}
		}

		private void FillCounterpartyBalanceNodes()
		{
			BalanceNodes.Clear();

			if(_counterpartyBalanceNodes is null)
			{
				return;
			}

			foreach(var keyPairValue in _counterpartyBalanceNodes)
			{
				var balanceNode = keyPairValue.Value;

				if(HideUnregisteredCounterparties && string.IsNullOrWhiteSpace(balanceNode.CounterpartyName))
				{
					continue;
				}

				if(balanceNode.CounterpartyBalance == balanceNode.CounterpartyBalance1C)
				{
					continue;
				}

				BalanceNodes.Add(keyPairValue.Value);
			}
		}

		#region UpdateCounterpartySummaryInfo
		private void UpdateCounterpartySummaryInfo()
		{
			OrdersTotalSum1C = _counterpartySettlementsReconciliation1C?.OrdersTotalSum.ToString(_decimalFormatString) ?? "-";
			PaymentsTotalSum1C = _counterpartySettlementsReconciliation1C?.PaymentsTotalSum.ToString(_decimalFormatString) ?? "-";
			Balance1C = _counterpartySettlementsReconciliation1C?.CounterpartyBalance.ToString(_decimalFormatString) ?? "-";
			OldBalance1C = _counterpartySettlementsReconciliation1C?.CounterpartyOldBalance.ToString(_decimalFormatString) ?? "-";

			if(_counterpartySettlementsReconciliation1C is null || SelectedClient is null)
			{
				OrdersTotalSumInDatabase = "-";
				PaymentsTotalSumInDatabase = "-";
				BalanceInDatabase = "-";
				OldBalanceInDatabase = "-";

				return;
			}

			OrdersTotalSumInDatabase = GetCounterpartyOrdersTotalSum(SelectedClient.Id).ToString(_decimalFormatString);
			PaymentsTotalSumInDatabase = GetCounterpartyPaymentsTotalSum(SelectedClient.Id, SelectedClient.INN).ToString(_decimalFormatString);
			BalanceInDatabase = GetCounterpartyBalance(SelectedClient.Id).ToString(_decimalFormatString);
			OldBalanceInDatabase = GetCounterpartyOldBalance(SelectedClient.Id).ToString(_decimalFormatString);
		}

		private decimal GetCounterpartyOrdersTotalSum(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartyOrdersActuaSums(_unitOfWork, counterpartyId, _availableOrderStatuses, false)
				.ToList().Sum();

			return sum;
		}

		private decimal GetCounterpartyPaymentsTotalSum(int counterpartyId, string counterpartyInn)
		{
			var sum = _paymentsRepository
				.GetCounterpartyPaymentsSums(_unitOfWork, counterpartyId, counterpartyInn)
				.ToList().Sum();

			return sum;
		}

		private decimal GetCounterpartyBalance(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, counterpartyId)
				.Select(x => x.Balance)
				.ToList().Sum();

			return sum;
		}

		private decimal GetCounterpartyOldBalance(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, counterpartyId, CounterpartySettlementsReconciliation1C.OldOrdersMaxDate)
				.Select(x => x.Balance)
				.ToList().Sum();

			return sum;
		}
		#endregion
	}
}
