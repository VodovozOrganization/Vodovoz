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
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using static Vodovoz.EntityRepositories.Orders.OrderRepository;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.CounterpartySettlementsReconciliation;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.TurnoverBalanceSheet;

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

		private CounterpartySettlementsReconciliation _counterpartySettlementsReconciliation;
		private TurnoverBalanceSheet _turnoverBalanceSheet;

		private IDictionary<int, OrderDiscrepanciesNode> _orderDiscrepanciesNodes;
		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> _paymentDiscrepanciesNodes;
		private IDictionary<string, CounterpartyBalanceNode> _counterpartyBalanceNodes;

		private DiscrepancyCheckMode _selectedCheckMode;
		private string _selectedFileName;
		private Domain.Client.Counterparty _selectedClient;
		private bool _isDiscrepanciesOnly;
		private bool _isClosedOrdersOnly;
		private bool _isExcludeOldData;

		private string _ordersTotalSumInFile;
		private string _paymentsTotalSumInFile;
		private string _totalDebtInFile;
		private string _oldDebtInFile;
		private string _ordersTotalSumInDatabase;
		private string _paymentsTotalSumInDatabase;
		private string _totalDebtInDatabase;
		private string _oldBalanceInDatabase;

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
				}
			}
		}

		public string OrdersTotalSumInFile
		{
			get => _ordersTotalSumInFile;
			set => SetField(ref _ordersTotalSumInFile, value);
		}

		public string PaymentsTotalSumInFile
		{
			get => _paymentsTotalSumInFile;
			set => SetField(ref _paymentsTotalSumInFile, value);
		}

		public string TotalDebtInFile
		{
			get => _totalDebtInFile;
			set => SetField(ref _totalDebtInFile, value);
		}

		public string OldDebtInFile
		{
			get => _oldDebtInFile;
			set => SetField(ref _oldDebtInFile, value);
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

		public string TotalDebtInDatabase
		{
			get => _totalDebtInDatabase;
			set => SetField(ref _totalDebtInDatabase, value);
		}

		public string OldBalanceInDatabase
		{
			get => _oldBalanceInDatabase;
			set => SetField(ref _oldBalanceInDatabase, value);
		}


		public bool CanReadFile => !string.IsNullOrWhiteSpace(_selectedFileName);

		public GenericObservableList<OrderDiscrepanciesNode> OrdersNodes { get; }
		public GenericObservableList<PaymentDiscrepanciesNode> PaymentsNodes { get; }
		public GenericObservableList<CounterpartyBalanceNode> BalanceNodes { get; }
		public GenericObservableList<Domain.Client.Counterparty> Clients { get; private set; }

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

			_counterpartySettlementsReconciliation = null;

			if(SelectedCheckMode == DiscrepancyCheckMode.ReconciliationByCounterparty)
			{
				CreateCounterpartySettlementsReconciliationFromXml();
			}

			if(SelectedCheckMode == DiscrepancyCheckMode.CommonReconciliation)
			{
				_turnoverBalanceSheet = CreateFromXls(SelectedFileName);
			}

			SetSelectedClient();
			UpdateOrderDiscrepanciesNodes();
			UpdatePaymentDiscrepanciesNodes();
			UpdateCounterpartyBalanceNodes();
			FillOrderNodes();
			FillPaymentNodes();
			FillCounterpartyBalanceNodes();
			UpdateCounterpartySummaryInfo();
		}

		private void CreateCounterpartySettlementsReconciliationFromXml()
		{
			try
			{
				_logger.LogInformation("Начинаем парсинг файла");

				_counterpartySettlementsReconciliation = CreateFromXlsx(SelectedFileName);

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
			Clients.Clear();

			if(_counterpartySettlementsReconciliation is null)
			{
				return;
			}

			if(string.IsNullOrEmpty(_counterpartySettlementsReconciliation.CounterpartyInn))
			{
				throw new Exception($"В указанном файле ИНН контрагента не найдено");
			}

			var counterparties = GetCounterpartiesByInn(_counterpartySettlementsReconciliation.CounterpartyInn);

			if(counterparties.Count != 1)
			{
				throw new Exception($"Найдено {counterparties.Count} контрагентов с ИНН {_counterpartySettlementsReconciliation.CounterpartyInn}. Должно быть 1");
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

			if(_counterpartySettlementsReconciliation is null || SelectedClient is null)
			{
				return;
			}

			var orderReconciliations = _counterpartySettlementsReconciliation.Orders;

			var allocations = GetAllocationsFromDatabase(SelectedClient.Id, orderReconciliations.Select(o => o.OrderId).ToList());

			var reconcillationOrderDiscrepanciesNodes = CreareOrderDiscrepanciesNodesFromReconciliations(orderReconciliations);

			MatchAllocationsWithOrderDiscrepanciesNodes(allocations, ref reconcillationOrderDiscrepanciesNodes);

			_orderDiscrepanciesNodes = reconcillationOrderDiscrepanciesNodes;
		}

		private IList<OrderWithAllocation> GetAllocationsFromDatabase(int clientId, IList<int> orderIds)
		{
			var allocations = _orderRepository.GetOrdersWithAllocationsOnDay(_unitOfWork, orderIds);
			var ordersMissingFromDocument = _orderRepository.GetOrdersWithAllocationsOnDay2(_unitOfWork, clientId, orderIds);

			return allocations.Concat(ordersMissingFromDocument).ToList();
		}

		private IDictionary<int, OrderDiscrepanciesNode> CreareOrderDiscrepanciesNodesFromReconciliations(
			IList<OrderReconciliation> orderReconciliations)
		{
			var orderDiscrepanciesNodes = new Dictionary<int, OrderDiscrepanciesNode>();

			foreach(var orderReconciliation in orderReconciliations)
			{
				var orderDiscrepanciesNode = new OrderDiscrepanciesNode
				{
					OrderId = orderReconciliation.OrderId,
					DocumentOrderSum = orderReconciliation.OrderSum,
					OrderDeliveryDateInDocument = orderReconciliation.OrderDeliveryDate
				};

				orderDiscrepanciesNodes.Add(orderDiscrepanciesNode.OrderId, orderDiscrepanciesNode);
			}

			return orderDiscrepanciesNodes;
		}

		private void MatchAllocationsWithOrderDiscrepanciesNodes(
			IList<OrderWithAllocation> allocations,
			ref IDictionary<int, OrderDiscrepanciesNode> orderDiscrepanciesNodes)
		{
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
						IsMissingFromDocument = allocation.IsMissingFromDocument
					};

					orderDiscrepanciesNodes.Add(orderDiscrepanciesNode.OrderId, orderDiscrepanciesNode);
				}
			}
		}
		#endregion

		#region UpdatePaymentDiscrepanciesNodes

		private void UpdatePaymentDiscrepanciesNodes()
		{
			_paymentDiscrepanciesNodes = null;

			if(_counterpartySettlementsReconciliation is null || SelectedClient is null)
			{
				return;
			}

			var paymentReconciliations = _counterpartySettlementsReconciliation.Payments;

			var paymentFromDatabase = GetPaymentsByNumbersFromDatabase(SelectedClient.INN, SelectedClient.Id);

			var paymentDiscrepanciesNodes = CrearePaymentDiscrepanciesNodesFromReconciliations(paymentReconciliations);

			MatchPaymentsInDatabaseWithOrderDiscrepanciesNodes(paymentFromDatabase, ref paymentDiscrepanciesNodes);

			_paymentDiscrepanciesNodes = paymentDiscrepanciesNodes;
		}

		private IList<PaymentNode> GetPaymentsByNumbersFromDatabase(
			string counterpartyInn,
			int counterpartyId)
		{
			var payments = _paymentsRepository
				.GetCounterpartyPaymentNodes(_unitOfWork, counterpartyId, counterpartyInn)
				.ToList();

			return payments;
		}

		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> CrearePaymentDiscrepanciesNodesFromReconciliations(
			IList<PaymentReconciliation> paymentReconciliations)
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

		private void MatchPaymentsInDatabaseWithOrderDiscrepanciesNodes(
			IList<PaymentNode> paymentNodesFromDatabase,
			ref IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
		{
			foreach(var paymentNode in paymentNodesFromDatabase)
			{
				if(paymentDiscrepanciesNodes.TryGetValue((paymentNode.PaymentNum, paymentNode.PaymentDate), out var paymentReconciliation))
				{
					paymentReconciliation.ProgramPaymentSum = paymentNode.PaymentSum;
					paymentReconciliation.IsManuallyCreated = paymentNode.IsManuallyCreated;
					paymentReconciliation.CounterpartyId = paymentNode.CounterpartyId;
					paymentReconciliation.CounterpartyName = paymentNode.CounterpartyName;
					paymentReconciliation.CounterpartyInn = paymentNode.CounterpartyInn;
					paymentReconciliation.PaymentPurpose = paymentNode.PaymentPurpose;
				}
				else
				{
					paymentDiscrepanciesNodes.Add(
						(paymentNode.PaymentNum, paymentNode.PaymentDate),
						new PaymentDiscrepanciesNode
						{
							PaymentNum = paymentNode.PaymentNum,
							PaymentDate = paymentNode.PaymentDate,
							ProgramPaymentSum = paymentNode.PaymentSum,
							IsManuallyCreated = paymentNode.IsManuallyCreated,
							CounterpartyId = paymentNode.CounterpartyId,
							CounterpartyName = paymentNode.CounterpartyName,
							CounterpartyInn = paymentNode.CounterpartyInn,
							PaymentPurpose = paymentNode.PaymentPurpose
						});
				}
			}
		}
		#endregion

		#region UpdateCounterpartyBalanceNodes
		private void UpdateCounterpartyBalanceNodes()
		{
			_paymentDiscrepanciesNodes = null;

			if(_turnoverBalanceSheet is null)
			{
				return;
			}

			var balancesFromSheet = _turnoverBalanceSheet.CounterpartyBalances;
			var balancesFromDatabase = GetCounterpartyBalancesFromDatabase();

			var counterpartyBalanceNodes = CreareCounterpartyBalanceNodesFromBalanceSheet(balancesFromSheet);

			MatchPaymentsInDatabaseWithOrderDiscrepanciesNodes(balancesFromDatabase, ref counterpartyBalanceNodes);

			_counterpartyBalanceNodes = counterpartyBalanceNodes;

			FillEmptyNamesInCounterpartyBalanceNodes();
		}

		private IList<CounterpartyCashlessBalanceNode> GetCounterpartyBalancesFromDatabase()
		{
			var balances = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses)
				.ToList();

			return balances;
		}

		private IDictionary<string, CounterpartyBalanceNode> CreareCounterpartyBalanceNodesFromBalanceSheet(IList<CounterpartyBalance> balances)
		{
			var balanceNodes = new Dictionary<string, CounterpartyBalanceNode>();

			foreach(var balance in balances)
			{
				if(balanceNodes.TryGetValue(balance.Inn, out var node))
				{
					node.CounterpartyBalance1C += (balance.Credit ?? 0) - (balance.Debit ?? 0);

					continue;
				}

				var balanceNode = new CounterpartyBalanceNode
				{
					CounterpartyInn = balance.Inn,
					CounterpartyBalance1C = (balance.Credit ?? 0) - (balance.Debit ?? 0)
				};

				balanceNodes.Add(balance.Inn, balanceNode);
			}

			return balanceNodes;
		}

		private void MatchPaymentsInDatabaseWithOrderDiscrepanciesNodes(
			IList<CounterpartyCashlessBalanceNode> balanceNodesFromDatabase,
			ref IDictionary<string, CounterpartyBalanceNode> counterpartyBalanceNodes)
		{
			foreach(var databaseBalanceNode in balanceNodesFromDatabase)
			{
				if(counterpartyBalanceNodes.TryGetValue(databaseBalanceNode.CounterpartyInn, out var balance))
				{
					balance.CounterpartyInn = databaseBalanceNode.CounterpartyInn;
					balance.CounterpartyName = $"{databaseBalanceNode.CounterpartyInn} {databaseBalanceNode.CounterpartyName}";
					balance.CounterpartyBalance = (-1) * databaseBalanceNode.Debt;
				}
				else
				{
					counterpartyBalanceNodes.Add(
						databaseBalanceNode.CounterpartyInn,
						new CounterpartyBalanceNode
						{
							CounterpartyInn = databaseBalanceNode.CounterpartyInn,
							CounterpartyName = $"{databaseBalanceNode.CounterpartyInn} {databaseBalanceNode.CounterpartyName}",
							CounterpartyBalance = (-1) * databaseBalanceNode.Debt
						});
				}
			}
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
					_counterpartyBalanceNodes[inn].CounterpartyName = $"{inn} {name}";
				}
				else
				{
					_counterpartyBalanceNodes[inn].CounterpartyName = $"{inn}";
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

				if(IsDiscrepanciesOnly && !orderNode.OrderSumDiscrepancy)
				{
					continue;
				}

				if(IsClosedOrdersOnly && orderNode.OrderStatus != OrderStatus.Closed)
				{
					continue;
				}

				if(IsExcludeOldData
					&& (orderNode.OrderDeliveryDateInDatabase < OldOrdersMaxDate.AddDays(1))
						|| (!orderNode.OrderDeliveryDateInDatabase.HasValue && orderNode.OrderDeliveryDate < OldOrdersMaxDate.AddDays(1)))
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
				PaymentsNodes.Add(keyPairValue.Value);
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
				BalanceNodes.Add(keyPairValue.Value);
			}
		}

		#region UpdateCounterpartySummaryInfo
		private void UpdateCounterpartySummaryInfo()
		{
			OrdersTotalSumInFile = _counterpartySettlementsReconciliation?.OrdersTotalSum.ToString(_decimalFormatString) ?? "-";
			PaymentsTotalSumInFile = _counterpartySettlementsReconciliation?.PaymentsTotalSum.ToString(_decimalFormatString) ?? "-";
			TotalDebtInFile = _counterpartySettlementsReconciliation?.CounterpartyTotalDebt.ToString(_decimalFormatString) ?? "-";
			OldDebtInFile = _counterpartySettlementsReconciliation?.CounterpartyOldBalance.ToString(_decimalFormatString) ?? "-";

			if(_counterpartySettlementsReconciliation is null || SelectedClient is null)
			{
				OrdersTotalSumInDatabase = "-";
				PaymentsTotalSumInDatabase = "-";
				TotalDebtInDatabase = "-";
				OldBalanceInDatabase = "-";

				return;
			}

			OrdersTotalSumInDatabase = GetCounterpartyOrdersTotalSum(SelectedClient.Id).ToString(_decimalFormatString);
			PaymentsTotalSumInDatabase = GetCounterpartyPaymentsTotalSum(SelectedClient.Id, SelectedClient.INN).ToString(_decimalFormatString);
			TotalDebtInDatabase = GetCounterpartyTotalDebt(SelectedClient.Id).ToString(_decimalFormatString);
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

		private decimal GetCounterpartyTotalDebt(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, counterpartyId)
				.Select(x => x.Debt)
				.ToList().Sum();

			return sum;
		}

		private decimal GetCounterpartyOldBalance(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, counterpartyId, OldOrdersMaxDate)
				.Select(x => x.Debt)
				.ToList().Sum();

			return (-1) * sum;
		}
		#endregion

		public class OrderDiscrepanciesNode
		{
			public int OrderId { get; set; }
			public DateTime? OrderDeliveryDateInDatabase { get; set; }
			public DateTime? OrderDeliveryDateInDocument { get; set; }
			public OrderStatus? OrderStatus { get; set; }
			public OrderPaymentStatus? OrderPaymentStatus { get; set; }
			public decimal DocumentOrderSum { get; set; }
			public decimal ProgramOrderSum { get; set; }
			public decimal AllocatedSum { get; set; }
			public bool IsMissingFromDocument { get; set; }
			public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;
			public DateTime? OrderDeliveryDate => OrderDeliveryDateInDatabase ?? OrderDeliveryDateInDocument;
		}

		public class PaymentDiscrepanciesNode
		{
			public int PaymentNum { get; set; }
			public DateTime PaymentDate { get; set; }
			public decimal DocumentPaymentSum { get; set; }
			public decimal ProgramPaymentSum { get; set; }
			public int CounterpartyId { get; set; }
			public string CounterpartyName { get; set; }
			public string CounterpartyInn { get; set; }
			public bool IsManuallyCreated { get; set; }
			public string PaymentPurpose { get; set; }
		}

		public class CounterpartyBalanceNode
		{
			public string CounterpartyInn { set; get; }
			public string CounterpartyName { get; set; }
			public decimal CounterpartyBalance { get; set; }
			public decimal CounterpartyBalance1C { get; set; }
		}

		public enum DiscrepancyCheckMode
		{
			[Display(Name = "Сверка по контрагенту")]
			ReconciliationByCounterparty,
			[Display(Name = "Общая сверка")]
			CommonReconciliation
		}
	}
}
