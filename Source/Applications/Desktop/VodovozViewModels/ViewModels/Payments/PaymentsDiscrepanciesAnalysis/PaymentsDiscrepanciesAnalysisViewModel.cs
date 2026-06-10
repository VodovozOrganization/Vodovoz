using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Organizations;

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
		private IList<OtherWriteOffDiscrepanciesNode> _otherWriteOffDiscrepanciesNodes;
		private IList<OtherIncomeNode> _otherIncomeNodes;
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
		private Organization _selectedOrganization;

		public PaymentsDiscrepanciesAnalysisViewModel(
			INavigationManager navigation,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository,
			ICounterpartyRepository counterpartyRepository,
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder,
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
			if(organizationViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(organizationViewModelEEVMBuilder));
			}

			_interactiveService = _commonServices.InteractiveService;

			Title = "Поиск расхождений в оплатах клиента";

			InitializeCommands();
			InitializeEntryViewModels(organizationViewModelEEVMBuilder);

			OrdersNodes = new GenericObservableList<OrderDiscrepanciesNode>();
			OrderDiscrepancyDuplicateNodes = new GenericObservableList<OrderDiscrepanciesNode>();
			PaymentsNodes = new GenericObservableList<PaymentDiscrepanciesNode>();
			OtherWriteOffNodes = new GenericObservableList<OtherWriteOffDiscrepanciesNode>();
			OtherIncomeNodes = new GenericObservableList<OtherIncomeNode>();
			BalanceNodes = new GenericObservableList<CounterpartyBalanceNode>();
			Clients = new GenericObservableList<Domain.Client.Counterparty>();

			_isClosedOrdersOnly = true;
		}

		private void InitializeEntryViewModels(ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder)
		{
			var organizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(_unitOfWork)
				.SetViewModel(this)
				.ForProperty(this, x => x.SelectedOrganization)
				.UseViewModelDialog<OrganizationViewModel>()
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.Finish();

			organizationViewModel.CanViewEntity = false;
			OrganizationViewModel = organizationViewModel;
		}

		private void InitializeCommands()
		{
			SetByCounterpartyCheckModeCommand = new DelegateCommand(SetByCounterpartyCheckMode);
			SetCommonReconciliationCheckModeCommand = new DelegateCommand(SetCommonReconciliationCheckMode);
			AnalyseDiscrepanciesCommand = new DelegateCommand(AnalyseDiscrepancies, () => CanReadFile);
			HelpCommand = new DelegateCommand(GetHelp);
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
					FillOtherWriteOffNodes();
					FillOtherIncomeNodes();
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
					FillOtherWriteOffNodes();
					FillOtherIncomeNodes();
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
					FillOtherWriteOffNodes();
					FillOtherIncomeNodes();
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

		public Organization SelectedOrganization
		{
			get => _selectedOrganization;
			set => SetField(ref _selectedOrganization, value);
		}

		public bool CanReadFile => !string.IsNullOrWhiteSpace(_selectedFileName);

		private static string Help =>
			$"Если не стоит фильтр Только закрытые, то строчки с заказами не в статусах <b>{OrderStatus.Shipped.GetEnumDisplayName()}</b>, " +
			$"<b>{OrderStatus.UnloadingOnStock.GetEnumDisplayName()}</b>, <b>{OrderStatus.Closed.GetEnumDisplayName()}</b> будут выделены желтым цветом.\n" +
			 "Для визуального отличия тех заказов, которые не попадут в акт сверки ДВ";

		public GenericObservableList<OrderDiscrepanciesNode> OrdersNodes { get; }
		public GenericObservableList<PaymentDiscrepanciesNode> PaymentsNodes { get; }
		public GenericObservableList<OtherWriteOffDiscrepanciesNode> OtherWriteOffNodes { get; }
		public GenericObservableList<OtherIncomeNode> OtherIncomeNodes { get; }
		public GenericObservableList<CounterpartyBalanceNode> BalanceNodes { get; }
		public GenericObservableList<Domain.Client.Counterparty> Clients { get; private set; }
		public GenericObservableList<OrderDiscrepanciesNode> OrderDiscrepancyDuplicateNodes { get; }
		public IEntityEntryViewModel OrganizationViewModel { get; private set; }

		#endregion

		#region Commands

		public DelegateCommand SetByCounterpartyCheckModeCommand { get; private set; }
		public DelegateCommand SetCommonReconciliationCheckModeCommand { get; private set; }
		public DelegateCommand AnalyseDiscrepanciesCommand { get; private set; }
		public DelegateCommand HelpCommand { get; private set; }

		#endregion

		private void SetByCounterpartyCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.ReconciliationByCounterparty;
		}

		private void SetCommonReconciliationCheckMode()
		{
			SelectedCheckMode = DiscrepancyCheckMode.CommonReconciliation;
		}

		private void GetHelp()
		{
			_interactiveService.ShowMessage(ImportanceLevel.Info, Help);
		}

		private void AnalyseDiscrepancies()
		{
			if(!CanReadFile)
			{
				return;
			}

			if(SelectedOrganization is null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Для сверки необходимо выбрать организацию");
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
			UpdateOtherWriteOffDiscrepanciesNodes();
			UpdateOtherIncomeNodes();
			UpdateCounterpartyBalanceNodes();
			FillOrderNodes();
			FillPaymentNodes();
			FillOtherWriteOffNodes();
			FillOtherIncomeNodes();
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
			catch(IOException ex)
			{
				var errorMessage = "Не удалось прочитать файл. Закройте его в Excel или другой программе и повторите попытку";

				_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

				_logger.LogDebug($"{errorMessage}: {ex.Message}");
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
			catch(IOException ex)
			{
				var errorMessage = "Не удалось прочитать файл. Закройте его в Excel или другой программе и повторите попытку";

				_interactiveService.ShowMessage(ImportanceLevel.Error, errorMessage);

				_logger.LogDebug($"{errorMessage}: {ex.Message}");
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

			var allocations = GetAllocationsFromDatabase(
				SelectedClient.Id,
				SelectedClient.INN,
				orders1C.Where(o => o.IsRecognizedOrder).Select(o => o.OrderId).ToList(),
				SelectedOrganization.Id);

			_orderDiscrepanciesNodes = CreateOrderDiscrepanciesNodes(orders1C, allocations);
		}

		private IList<OrderWithAllocation> GetAllocationsFromDatabase(
			int clientId,
			string clientInn,
			IList<int> orderIds,
			int organizationId)
		{
			var allocations = orderIds.Any()
				? _orderRepository.GetOrdersWithAllocationsOnDayByOrdersIds(_unitOfWork, orderIds, organizationId)
				: new List<OrderWithAllocation>();

			var ordersMissingFromDocumentAllocatedToClient =
				_orderRepository.GetOrdersWithAllocationsOnDayByCounterparty(_unitOfWork, clientId, orderIds, organizationId);
			var ordersMissingFromDocumentAllocatedToAnotherClient =
				_orderRepository.GetAllocationsToOrdersWithAnotherClient(_unitOfWork, clientId, clientInn, orderIds, organizationId);

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
					OrderDeliveryDateInDocument = orderReconciliation.OrderDeliveryDate,
					DocumentName = orderReconciliation.DocumentName
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

			var paymentFromDatabase = GetCounterpartyPaymentsFromDatabase(SelectedClient.INN, SelectedClient.Id, SelectedOrganization.Id);

			_paymentDiscrepanciesNodes = CreatePaymentDiscrepanciesNodes(payments1C, paymentFromDatabase);
		}

		private IList<PaymentNode> GetCounterpartyPaymentsFromDatabase(
			string counterpartyInn,
			int counterpartyId,
			int organizationId)
		{
			var payments = _paymentsRepository
				.GetCounterpartyPaymentNodes(_unitOfWork, counterpartyId, counterpartyInn, organizationId)
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

		#region OtherWriteOffs
		private void UpdateOtherWriteOffDiscrepanciesNodes()
		{
			_otherWriteOffDiscrepanciesNodes = null;

			if(_counterpartySettlementsReconciliation1C is null || SelectedClient is null)
			{
				return;
			}

			var writeOffs1C = _counterpartySettlementsReconciliation1C.OtherWriteOffs;
			var writeOffsFromDatabase = GetCounterpartyPaymentWriteOffsFromDatabase(SelectedClient.Id, SelectedOrganization.Id);

			_otherWriteOffDiscrepanciesNodes = CreateOtherWriteOffDiscrepanciesNodes(writeOffs1C, writeOffsFromDatabase);
		}

		private IList<PaymentWriteOffNode> GetCounterpartyPaymentWriteOffsFromDatabase(int counterpartyId, int organizationId)
		{
			return _paymentsRepository
				.GetCounterpartyPaymentWriteOffNodes(_unitOfWork, counterpartyId, organizationId);
		}

		private IList<OtherWriteOffDiscrepanciesNode> CreateOtherWriteOffDiscrepanciesNodes(
			IList<OtherWriteOffReconciliation1C> writeOffs1C,
			IList<PaymentWriteOffNode> writeOffsFromDatabase)
		{
			var nodes = new List<OtherWriteOffDiscrepanciesNode>();
			var unmatchedWriteOffsFromDatabase = writeOffsFromDatabase.ToList();

			foreach(var writeOff1C in writeOffs1C)
			{
				var node = new OtherWriteOffDiscrepanciesNode
				{
					DocumentName = writeOff1C.DocumentName,
					DocumentNumber = writeOff1C.DocumentNumber,
					DocumentDate = writeOff1C.DocumentDate,
					DocumentWriteOffSum = writeOff1C.WriteOffSum
				};

				var exactMatch = FindPaymentWriteOffMatch(writeOff1C, unmatchedWriteOffsFromDatabase, true);
				var match = exactMatch ?? FindPaymentWriteOffMatch(writeOff1C, unmatchedWriteOffsFromDatabase, false);

				if(match != null)
				{
					FillOtherWriteOffNodeFromDatabase(node, match);
					node.IsMatchedWithoutNumber = exactMatch is null;
					unmatchedWriteOffsFromDatabase.Remove(match);
				}

				nodes.Add(node);
			}

			foreach(var writeOffFromDatabase in unmatchedWriteOffsFromDatabase)
			{
				var node = new OtherWriteOffDiscrepanciesNode();
				FillOtherWriteOffNodeFromDatabase(node, writeOffFromDatabase);
				nodes.Add(node);
			}

			return nodes;
		}

		private PaymentWriteOffNode FindPaymentWriteOffMatch(
			OtherWriteOffReconciliation1C writeOff1C,
			IList<PaymentWriteOffNode> writeOffsFromDatabase,
			bool matchByNumber)
		{
			if(!writeOff1C.DocumentDate.HasValue)
			{
				return null;
			}

			return writeOffsFromDatabase.FirstOrDefault(writeOff =>
				writeOff.Date.Date == writeOff1C.DocumentDate.Value.Date
				&& writeOff.Sum == writeOff1C.WriteOffSum
				&& (!matchByNumber
					|| writeOff1C.DocumentNumber.HasValue && writeOff.PaymentNumber == writeOff1C.DocumentNumber.Value));
		}

		private void FillOtherWriteOffNodeFromDatabase(
			OtherWriteOffDiscrepanciesNode node,
			PaymentWriteOffNode paymentWriteOff)
		{
			node.PaymentWriteOffId = paymentWriteOff.Id;
			node.PaymentWriteOffNumber = paymentWriteOff.PaymentNumber;
			node.PaymentWriteOffDate = paymentWriteOff.Date;
			node.ProgramWriteOffSum = paymentWriteOff.Sum;
			node.Reason = paymentWriteOff.Reason;
		}
		#endregion

		#region OtherIncomes
		private void UpdateOtherIncomeNodes()
		{
			_otherIncomeNodes = null;

			if(_counterpartySettlementsReconciliation1C is null || SelectedClient is null)
			{
				return;
			}

			_otherIncomeNodes = _counterpartySettlementsReconciliation1C.OtherIncomes
				.Select(income => new OtherIncomeNode
				{
					DocumentName = income.DocumentName,
					DocumentNumber = income.DocumentNumber,
					DocumentDate = income.DocumentDate,
					IncomeSum = income.IncomeSum
				})
				.ToList();
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
					.GetCounterpartiesCashlessBalance(
						_unitOfWork,
						_availableOrderStatuses,
						maxDeliveryDate: CommonReconciliationDataMaxDate.Value,
						organizationId: SelectedOrganization.Id)
					.ToList();
			}

			return _counterpartyRepository
				.GetCounterpartiesCashlessBalance(_unitOfWork, _availableOrderStatuses, organizationId: SelectedOrganization.Id)
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

		private void FillOtherWriteOffNodes()
		{
			OtherWriteOffNodes.Clear();

			if(_otherWriteOffDiscrepanciesNodes is null)
			{
				return;
			}

			foreach(var writeOffNode in _otherWriteOffDiscrepanciesNodes)
			{
				if(IsDiscrepanciesOnly && !writeOffNode.WriteOffDiscrepancy)
				{
					continue;
				}

				if(IsExcludeOldData
					&& writeOffNode.WriteOffDate < CounterpartySettlementsReconciliation1C.OldOrdersMaxDate.AddDays(1))
				{
					continue;
				}

				OtherWriteOffNodes.Add(writeOffNode);
			}
		}

		private void FillOtherIncomeNodes()
		{
			OtherIncomeNodes.Clear();

			if(_otherIncomeNodes is null)
			{
				return;
			}

			foreach(var incomeNode in _otherIncomeNodes)
			{
				if(IsExcludeOldData
					&& incomeNode.DocumentDate < CounterpartySettlementsReconciliation1C.OldOrdersMaxDate.AddDays(1))
				{
					continue;
				}

				OtherIncomeNodes.Add(incomeNode);
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
				.GetCounterpartyOrdersActuaSums(_unitOfWork, counterpartyId, _availableOrderStatuses, false, organizationId: SelectedOrganization.Id)
				.AsEnumerable()
				.Sum();

			return sum;
		}

		private decimal GetCounterpartyPaymentsTotalSum(int counterpartyId, string counterpartyInn)
		{
			var sum = _paymentsRepository
				.GetCounterpartyPaymentsSums(_unitOfWork, counterpartyId, counterpartyInn, SelectedOrganization.Id)
				.AsEnumerable()
				.Sum();

			return sum;
		}

		private decimal GetCounterpartyBalance(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(
					_unitOfWork,
					_availableOrderStatuses,
					counterpartyId,
					organizationId: SelectedOrganization.Id)
				.Select(x => x.Balance)
				.AsEnumerable()
				.Sum();

			return sum;
		}

		private decimal GetCounterpartyOldBalance(int counterpartyId)
		{
			var sum = _counterpartyRepository
				.GetCounterpartiesCashlessBalance(
					_unitOfWork,
					_availableOrderStatuses,
					counterpartyId,
					CounterpartySettlementsReconciliation1C.OldOrdersMaxDate,
					SelectedOrganization.Id)
				.Select(x => x.Balance)
				.AsEnumerable()
				.Sum();

			return sum;
		}
		#endregion
	}
}
