using Microsoft.Extensions.Logging;
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
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.ReconciliationOfMutualSettlements;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel : DialogViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<PaymentsDiscrepanciesAnalysisViewModel> _logger;

		private IDictionary<int, OrderDiscrepanciesNode> _orderDiscrepanciesNodes = new Dictionary<int, OrderDiscrepanciesNode>();
		private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> _paymentDiscrepanciesNodes =
			new Dictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode>();

		private ReconciliationOfMutualSettlements _reconciliationOfMutualSettlements;

		private DiscrepancyCheckMode _selectedCheckMode;
		private string _selectedFileName;
		private Domain.Client.Counterparty _selectedClient;
		private bool _isDiscrepanciesOnly;
		private bool _isClosedOrdersOnly = true;
		private bool _isExcludeOldData;

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
			set => SetField(ref _isDiscrepanciesOnly, value);
		}

		public bool IsClosedOrdersOnly
		{
			get => _isClosedOrdersOnly;
			set => SetField(ref _isClosedOrdersOnly, value);
		}

		public bool IsExcludeOldData
		{
			get => _isExcludeOldData;
			set => SetField(ref _isExcludeOldData, value);
		}

		public bool CanReadFile => !string.IsNullOrWhiteSpace(_selectedFileName);

		public GenericObservableList<OrderDiscrepanciesNode> OrdersNodes { get; } =
			new GenericObservableList<OrderDiscrepanciesNode>();

		public GenericObservableList<PaymentDiscrepanciesNode> PaymentsNodes { get; } =
			new GenericObservableList<PaymentDiscrepanciesNode>();

		public GenericObservableList<CounterpartyBalanceNode> CounterpartyBalanceNodes { get; } =
			new GenericObservableList<CounterpartyBalanceNode>();

		public GenericObservableList<Domain.Client.Counterparty> Clients { get; private set; } =
			new GenericObservableList<Domain.Client.Counterparty>();

		#endregion Settings

		#region Commands

		public DelegateCommand SetByCounterpartyCheckModeCommand { get; }
		public DelegateCommand SetCommonReconciliationCheckModeCommand { get; }
		public DelegateCommand AnalyseDiscrepanciesCommand { get; }

		#endregion Commands

		private void SetByCounterpartyCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.ByCounterparty;
		}

		private void SetCommonReconciliationCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.CommonReconciliation;
		}

		private void AnalyseDiscrepancies()
		{
			if(SelectedCheckMode == DiscrepancyCheckMode.ByCounterparty)
			{
				if(!ParseFile())
				{
					return;
				}

				if(string.IsNullOrEmpty(_reconciliationOfMutualSettlements.ClientInn))
				{
					var errorMessage = "В представленном файле не найден ИНН контрагента";

					_logger.LogDebug(errorMessage);
					_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

					return;
				}

				UpdateClientsList();
				SetSelectedClient();

				ProcessData();

				return;
			}

			if(SelectedCheckMode == DiscrepancyCheckMode.CommonReconciliation)
			{
				return;
			}

			throw new NotSupportedException("Неизветный режим поиска расхождений");
		}

		private bool ParseFile()
		{
			try
			{
				_logger.LogInformation("Начинаем парсинг файла");

				_reconciliationOfMutualSettlements =
					ReconciliationOfMutualSettlements.CreateReconciliationOfMutualSettlementsFromXml(
						SelectedFileName,
						_unitOfWork,
						_orderRepository,
						_paymentsRepository,
						_counterpartyRepository);

				_logger.LogInformation("Парсинг файла закончен успешно");

				return true;
			}
			catch(Exception ex)
			{
				var errorMessage = $"При парсинге файла возникла ошибка";

				_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

				_logger.LogDebug($"{errorMessage}: {ex.Message}");

				return false;
			}
		}

		private void UpdateClientsList()
		{
			if(_reconciliationOfMutualSettlements is null)
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(_reconciliationOfMutualSettlements.ClientInn))
			{
				return;
			}

			_logger.LogInformation("Подбираем клиентов по имени из акта сверки");
			var clients = _counterpartyRepository.GetCounterpartiesByINN(_unitOfWork, _reconciliationOfMutualSettlements.ClientInn);

			Clients = new GenericObservableList<Domain.Client.Counterparty>(clients);

			OnPropertyChanged(nameof(Clients));
			_logger.LogInformation($"Подобрали клиентов, количество {clients.Count}");
		}

		private void SetSelectedClient()
		{
			if(Clients.Count == 1)
			{
				SelectedClient = Clients[0];

				return;
			}

			SelectedClient = null;

			_interactiveService.ShowMessage(ImportanceLevel.Error, $"Найдено {Clients.Count}");
		}

		private void ProcessData()
		{
			if(SelectedClient is null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Info,
					"Не выбран клиент, по которому нужно провести обработку данных");

				return;
			}

			_logger.LogInformation("Смотрим в базе заказы...");
			var allocations = _orderRepository.GetOrdersWithAllocationsOnDay(_unitOfWork, _reconciliationOfMutualSettlements.OrderIds);
			var ordersMissingFromDocument = _orderRepository.GetOrdersWithAllocationsOnDay2(
				_unitOfWork, SelectedClient.Id, _reconciliationOfMutualSettlements.OrderIds);

			ProcessingOrders(allocations.Concat(ordersMissingFromDocument));
			FillOrderNodes();

			_logger.LogInformation("Смотрим в базе платежи...");
			var payments = _paymentsRepository.GetPaymentsByNumbers(_unitOfWork, _reconciliationOfMutualSettlements.PaymentNums, SelectedClient.INN);

			ProcessingPayments(payments);
			FillPaymentNodes();
		}

		private void ProcessingOrders(IEnumerable<OrderRepository.OrderWithAllocation> allocations)
		{
			_logger.LogInformation("Сопоставляем данные по заказам");
			foreach(var allocation in allocations)
			{
				if(_orderDiscrepanciesNodes.TryGetValue(allocation.OrderId, out var node))
				{
					node.ProgramOrderSum = allocation.OrderSum;
					node.AllocatedSum = allocation.OrderAllocation;
					node.OrderDeliveryDate = allocation.OrderDeliveryDate;
					node.OrderStatus = allocation.OrderStatus;
					node.OrderPaymentStatus = allocation.OrderPaymentStatus;
					node.IsMissingFromDocument = allocation.IsMissingFromDocument;
				}
				else
				{
					_orderDiscrepanciesNodes.Add(
						allocation.OrderId,
						new OrderDiscrepanciesNode
						{
							OrderId = allocation.OrderId,
							AllocatedSum = allocation.OrderAllocation,
							ProgramOrderSum = allocation.OrderSum,
							OrderDeliveryDate = allocation.OrderDeliveryDate,
							OrderStatus = allocation.OrderStatus,
							OrderPaymentStatus = allocation.OrderPaymentStatus,
							IsMissingFromDocument = allocation.IsMissingFromDocument
						});
				}
			}
		}

		private void ProcessingPayments(IList<PaymentNode> paymentNodes)
		{
			_logger.LogInformation("Сопоставляем данные по платежам");
			foreach(var paymentNode in paymentNodes)
			{
				if(_paymentDiscrepanciesNodes.TryGetValue((paymentNode.PaymentNum, paymentNode.PaymentDate), out var node))
				{
					node.ProgramPaymentSum = paymentNode.PaymentSum;
					node.IsManuallyCreated = paymentNode.IsManuallyCreated;
					node.CounterpartyId = paymentNode.CounterpartyId;
					node.CounterpartyName = paymentNode.CounterpartyName;
					node.CounterpartyInn = paymentNode.CounterpartyInn;
					node.PaymentPurpose = paymentNode.PaymentPurpose;
				}
				else
				{
					_paymentDiscrepanciesNodes.Add(
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

		private void FillOrderNodes()
		{
			OrdersNodes.Clear();
			foreach(var keyPairValue in _orderDiscrepanciesNodes)
			{
				OrdersNodes.Add(keyPairValue.Value);
			}
		}

		private void FillPaymentNodes()
		{
			PaymentsNodes.Clear();
			foreach(var keyPairValue in _paymentDiscrepanciesNodes)
			{
				PaymentsNodes.Add(keyPairValue.Value);
			}
		}

		public class CounterpartyBalanceNode
		{
			public string CounterpartyName { get; set; }
			public decimal CounterpartyBalance { get; set; }
			public decimal CounterpartyBalance1C { get; set; }
		}

		public enum DiscrepancyCheckMode
		{
			ByCounterparty,
			CommonReconciliation
		}
	}
}
