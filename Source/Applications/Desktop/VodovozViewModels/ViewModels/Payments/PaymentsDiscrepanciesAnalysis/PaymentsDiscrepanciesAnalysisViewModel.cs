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
				if (SetField(ref _isExcludeOldData, value))
				{
					FillOrderNodes();
				}
			}
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
				if(!CreateReconciliationOfMutualSettlementsFromXml())
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

				SetSelectedClient();
				FillOrderNodes();
				FillPaymentNodes();

				return;
			}

			if(SelectedCheckMode == DiscrepancyCheckMode.CommonReconciliation)
			{
				return;
			}

			throw new NotSupportedException("Неизветный режим поиска расхождений");
		}

		private bool CreateReconciliationOfMutualSettlementsFromXml()
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

		private void SetSelectedClient()
		{
			if(_reconciliationOfMutualSettlements is null)
			{
				return;
			}

			Clients.Clear();

			foreach(var counterparty in _reconciliationOfMutualSettlements.Counterparties)
			{
				Clients.Add(counterparty);
			}

			OnPropertyChanged(nameof(Clients));

			if(Clients.Count == 1)
			{
				SelectedClient = Clients.First();

				return;
			}

			SelectedClient = null;

			_interactiveService.ShowMessage(ImportanceLevel.Error, $"Найдено {Clients.Count}");
		}

		private void FillOrderNodes()
		{
			if(_reconciliationOfMutualSettlements == null)
			{
				return;
			}

			OrdersNodes.Clear();

			foreach(var keyPairValue in _reconciliationOfMutualSettlements.OrderNodes)
			{
				var orderNode = keyPairValue.Value;

				if(IsDiscrepanciesOnly && !orderNode.OrderSumDiscrepancy)
				{
					continue;
				}

				if(IsClosedOrdersOnly && orderNode.OrderStatus != Domain.Orders.OrderStatus.Closed)
				{
					continue;
				}

				if(IsExcludeOldData && orderNode.OrderDeliveryDateInDatabase < OldOrdersDate.AddDays(1))
				{
					continue;
				}

				OrdersNodes.Add(orderNode);
			}
		}

		private void FillPaymentNodes()
		{
			if(_reconciliationOfMutualSettlements == null)
			{
				return;
			}

			PaymentsNodes.Clear();

			foreach(var keyPairValue in _reconciliationOfMutualSettlements.PaymentNodes)
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
