using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentsDiscrepanciesAnalysisViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<PaymentsDiscrepanciesAnalysisViewModel> _logger;
		private readonly CsvParser _csvParser;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private string _selectedFileName;
		
		private IDictionary<int, OrderDiscrepanciesNode> _orderDiscrepanciesNodes = new Dictionary<int, OrderDiscrepanciesNode>();
		private IDictionary<int, PaymentDiscrepanciesNode> _paymentDiscrepanciesNodes = new Dictionary<int, PaymentDiscrepanciesNode>();
		private Domain.Client.Counterparty _selectedClient;

		public PaymentsDiscrepanciesAnalysisViewModel(
			ILogger<PaymentsDiscrepanciesAnalysisViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			CsvParser csvParser,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository,
			INavigationManager navigationManager) : base(navigationManager)
		{
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateParseCommand();
			CreateGetClientsCommand();
			CreateProcessingDataCommand();
		}

		private void CreateParseCommand()
		{
			ParseCommand = new DelegateCommand(
				() =>
				{
					_csvParser.Parse(SelectedFileName, _orderDiscrepanciesNodes, _paymentDiscrepanciesNodes);
					_logger.LogInformation("Распарсили файл");
				}
			);
		}
		
		private void CreateGetClientsCommand()
		{
			GetClientsCommand = new DelegateCommand(
				() =>
				{
					if(string.IsNullOrWhiteSpace(_csvParser.ClientInn))
					{
						return;
					}
					
					_logger.LogInformation("Подбираем клиентов по имени из акта сверки");
					var clients = _counterpartyRepository.GetCounterpartiesByINN(_unitOfWork, _csvParser.ClientInn);

					foreach(var client in clients)
					{
						Clients.Add(client);
					}
					OnPropertyChanged(nameof(Clients));
					_logger.LogInformation($"Подобрали клиентов, количество {clients.Count}");
				}
			);
		}
		
		private void CreateProcessingDataCommand()
		{
			ProcessingDataCommand = new DelegateCommand(
				() =>
				{
					if(SelectedClient is null)
					{
						return;
					}
					
					_logger.LogInformation("Смотрим в базе заказы...");
					var allocations = _orderRepository.GetOrdersWithAllocationsOnDay(_unitOfWork, _csvParser.OrderIds);
					var ordersMissingFromDocument = _orderRepository.GetOrdersWithAllocationsOnDay2(
						_unitOfWork, SelectedClient.Id, _csvParser.OrderIds);

					ProcessingOrders(allocations.Concat(ordersMissingFromDocument));
					FillOrderNodes();
					
					_logger.LogInformation("Смотрим в базе платежи...");
					var payments = _paymentsRepository.GetPaymentsByNumbers(_unitOfWork, _csvParser.PaymentNums, SelectedClient.INN);
					
					ProcessingPayments(payments);
					FillPaymentNodes();
				}
			);
		}

		private void FillOrderNodes()
		{
			OrdersNodes.Clear();
			foreach(var keyPairValue in _orderDiscrepanciesNodes)
			{
				OrdersNodes.Add(keyPairValue.Value);
			}
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
							IsMissingFromDocument = allocation.IsMissingFromDocument
						});
				}
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

		private void ProcessingPayments(IList<PaymentNode> paymentNodes)
		{
			_logger.LogInformation("Сопоставляем данные по платежам");
			foreach(var paymentNode in paymentNodes)
			{
				if(_paymentDiscrepanciesNodes.TryGetValue(paymentNode.PaymentNum, out var node))
				{
					node.ProgramPaymentSum = paymentNode.PaymentSum;
					node.IsManuallyCreated = paymentNode.IsManuallyCreated;
					node.CounterpartyId = paymentNode.CounterpartyId;
					node.CounterpartyName = paymentNode.CounterpartyName;
					node.CounterpartyInn = paymentNode.CounterpartyInn;
				}
				else
				{
					_paymentDiscrepanciesNodes.Add(
						paymentNode.PaymentNum,
						new PaymentDiscrepanciesNode
						{
							PaymentNum = paymentNode.PaymentNum,
							ProgramPaymentSum = paymentNode.PaymentSum,
							IsManuallyCreated = paymentNode.IsManuallyCreated,
							CounterpartyId = paymentNode.CounterpartyId,
							CounterpartyName = paymentNode.CounterpartyName,
							CounterpartyInn = paymentNode.CounterpartyInn
						});
				}
			}
		}

		public DelegateCommand ParseCommand { get; private set; }
		public DelegateCommand GetClientsCommand { get; private set; }
		public DelegateCommand ProcessingDataCommand { get; private set; }

		public GenericObservableList<OrderDiscrepanciesNode> OrdersNodes { get; } =
			new GenericObservableList<OrderDiscrepanciesNode>();
		
		public GenericObservableList<PaymentDiscrepanciesNode> PaymentsNodes { get; } =
			new GenericObservableList<PaymentDiscrepanciesNode>();
		
		public GenericObservableList<Domain.Client.Counterparty> Clients { get; } = new GenericObservableList<Domain.Client.Counterparty>();

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

		public bool CanReadFile => !string.IsNullOrWhiteSpace(_selectedFileName);

		public Domain.Client.Counterparty SelectedClient
		{
			get => _selectedClient;
			set => SetField(ref _selectedClient, value);
		}
	}

	public class CsvParser
	{
		private const string _numberPattern = @"([0-9]{1,})";
		private const string _datePattern = @"([0-9]{2}.[0-9]{2}.[0-9]{4})";
		
		public string ClientInn { get; private set; }
		public IList<int> OrderIds { get; } = new List<int>();
		public IList<int> PaymentNums { get; } = new List<int>();
		
		public void Parse(
			string fileName,
			IDictionary<int, OrderDiscrepanciesNode> orderDiscrepanciesNodes,
			IDictionary<int, PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
		{
			OrderIds.Clear();
			orderDiscrepanciesNodes.Clear();
			PaymentNums.Clear();
			paymentDiscrepanciesNodes.Clear();
			
			using(var reader = new StreamReader(fileName, Encoding.GetEncoding(1251)))
			{
				string line;
				
				while((line = reader.ReadLine()) != null)
				{
					if(string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					var data = line.Split(';');
					
					if(data.Length < 3)
					{
						continue;
					}
					
					if(data[3].StartsWith("(Сосновцев"))
					{
						ClientInn = ParseClientInnFromString(data[11]);
						continue;
					}
					
					DateTime.TryParse(data[1], out var date);

					if(date == default)
					{
						continue;
					}

					if(data[2].StartsWith("Продажа"))
					{
						CreateOrderNode(data, orderDiscrepanciesNodes);
					}
					else
					{
						CreatePaymentNode(data, paymentDiscrepanciesNodes);
					}
				}
			}
		}

		private void CreateOrderNode(string[] data, IDictionary<int, OrderDiscrepanciesNode> orderDiscrepanciesNodes)
		{
			var node = new OrderDiscrepanciesNode
			{
				OrderId = ParseNumberFromString(data[2]),
				DocumentOrderSum = decimal.Parse(data[4])
			};

			orderDiscrepanciesNodes.Add(node.OrderId, node);
			OrderIds.Add(node.OrderId);
		}
		
		private void CreatePaymentNode(string[] data, IDictionary<int, PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
		{
			var node = new PaymentDiscrepanciesNode
			{
				PaymentNum = ParseNumberFromString(data[2]),
				PaymentDate = ParseDateFromString(data[2]),
				DocumentPaymentSum = decimal.Parse(data[6])
			};

			paymentDiscrepanciesNodes.Add(node.PaymentNum, node);
			PaymentNums.Add(node.PaymentNum);
		}

		private int ParseNumberFromString(string str)
		{
			var matches = Regex.Matches(str, _numberPattern);
			return int.Parse(matches[0].Value);
		}
		
		private DateTime ParseDateFromString(string str)
		{
			var matches = Regex.Matches(str, _datePattern);
			return DateTime.Parse(matches[0].Value);
		}
		
		private string ParseClientInnFromString(string str)
		{
			var matches = Regex.Matches(str, _numberPattern);
			return matches[0].Value;
		}
	}

	public class OrderDiscrepanciesNode
	{
		public int OrderId { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public OrderStatus? OrderStatus { get; set; }
		public decimal DocumentOrderSum { get; set; }
		public decimal ProgramOrderSum { get; set; }
		public decimal AllocatedSum { get; set; }
		public bool IsMissingFromDocument { get; set; }
		public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;
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
	}
}
