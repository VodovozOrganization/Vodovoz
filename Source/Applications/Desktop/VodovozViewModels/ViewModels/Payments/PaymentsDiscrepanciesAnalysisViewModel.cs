using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentsDiscrepanciesAnalysisViewModel : DialogViewModelBase
	{
		private DiscrepancyCheckMode _selectedCheckMode;
		private string _selectedFileName;
		private Domain.Client.Counterparty _selectedClient;
		private bool _isDiscrepanciesOnly;
		private bool _isClosedOrdersOnly = true;
		private bool _isExcludeOldData;

		public PaymentsDiscrepanciesAnalysisViewModel(
			INavigationManager navigation) : base(navigation)
		{
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

		}

		public class OrderDiscrepanciesNode
		{
			public int OrderId { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public OrderStatus? OrderStatus { get; set; }
			public OrderPaymentStatus? OrderPaymentStatus { get; set; }
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
			public string PaymentPurpose { get; set; }
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
				IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
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

			private void CreatePaymentNode(string[] data, IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
			{
				var node = new PaymentDiscrepanciesNode
				{
					PaymentNum = ParseNumberFromString(data[2]),
					PaymentDate = ParseDateFromString(data[2]),
					DocumentPaymentSum = decimal.Parse(data[6])
				};

				paymentDiscrepanciesNodes.Add((node.PaymentNum, node.PaymentDate), node);
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
	}
}
