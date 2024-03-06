using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.CounterpartySettlementsReconciliation;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.TurnoverBalanceSheet;

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

		private CounterpartySettlementsReconciliation _counterpartySettlementsReconciliation;
		private TurnoverBalanceSheet _turnoverBalanceSheet;

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

		public string OrdersTotalSumInFile => _counterpartySettlementsReconciliation?.OrdersTotalSumInFile.ToString("# ##0.00") ?? "-";
		public string PaymentsTotalSumInFile => _counterpartySettlementsReconciliation?.PaymentsTotalSumInFile.ToString("# ##0.00") ?? "-";
		public string TotalDebtInFile => _counterpartySettlementsReconciliation?.TotalDebtInFile.ToString("# ##0.00") ?? "-";
		public string OldDebtInFile => _counterpartySettlementsReconciliation?.OldDebtInFile.ToString("# ##0.00") ?? "-";
		public string OrdersTotalSumInDatabase => _counterpartySettlementsReconciliation?.OrdersTotalSumInDatabase.ToString("# ##0.00") ?? "-";
		public string PaymentsTotalSumInDatabase => _counterpartySettlementsReconciliation?.PaymentsTotalSumInDatabase.ToString("# ##0.00") ?? "-";
		public string TotalDebtInDatabase => _counterpartySettlementsReconciliation?.TotalDebtInDatabase.ToString("# ##0.00") ?? "-";
		public string OldDebtInDatabase => _counterpartySettlementsReconciliation?.OldDebtInDatabase.ToString("# ##0.00") ?? "-";

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
				CreateReconciliationOfMutualSettlementsFromXml();
			}

			if(SelectedCheckMode == DiscrepancyCheckMode.CommonReconciliation)
			{
				_turnoverBalanceSheet = CreateFromXls(SelectedFileName);
				return;
			}

			SetSelectedClient();
			FillOrderNodes();
			FillPaymentNodes();
			UpdateCounterpartySummaryInfo();
		}

		private void CreateReconciliationOfMutualSettlementsFromXml()
		{
			try
			{
				_logger.LogInformation("Начинаем парсинг файла");

				_counterpartySettlementsReconciliation =
					CreateFromXls(
						_unitOfWork,
						_orderRepository,
						_paymentsRepository,
						_counterpartyRepository,
						SelectedFileName);

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

			Clients.Add(_counterpartySettlementsReconciliation.Counterparty);

			OnPropertyChanged(nameof(Clients));

			SelectedClient = _counterpartySettlementsReconciliation.Counterparty;
		}

		private void FillOrderNodes()
		{
			OrdersNodes.Clear();

			if(_counterpartySettlementsReconciliation is null)
			{
				return;
			}

			foreach(var keyPairValue in _counterpartySettlementsReconciliation.OrderNodes)
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

				if(IsExcludeOldData && orderNode.OrderDeliveryDateInDatabase < OldOrdersMaxDate.AddDays(1))
				{
					continue;
				}

				OrdersNodes.Add(orderNode);
			}
		}

		private void FillPaymentNodes()
		{
			PaymentsNodes.Clear();

			if(_counterpartySettlementsReconciliation is null)
			{
				return;
			}

			foreach(var keyPairValue in _counterpartySettlementsReconciliation.PaymentNodes)
			{
				PaymentsNodes.Add(keyPairValue.Value);
			}
		}

		private void UpdateCounterpartySummaryInfo()
		{
			OnPropertyChanged(nameof(OrdersTotalSumInFile));
			OnPropertyChanged(nameof(PaymentsTotalSumInFile));
			OnPropertyChanged(nameof(TotalDebtInFile));
			OnPropertyChanged(nameof(OldDebtInFile));
			OnPropertyChanged(nameof(OrdersTotalSumInDatabase));
			OnPropertyChanged(nameof(PaymentsTotalSumInDatabase));
			OnPropertyChanged(nameof(TotalDebtInDatabase));
			OnPropertyChanged(nameof(OldDebtInDatabase));
		}

		public class CounterpartyBalanceNode
		{
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
