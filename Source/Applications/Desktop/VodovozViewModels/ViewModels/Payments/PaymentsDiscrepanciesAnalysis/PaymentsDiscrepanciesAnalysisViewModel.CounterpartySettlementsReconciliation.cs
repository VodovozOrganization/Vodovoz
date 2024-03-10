using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Сверка взаиморасчетов по контрагенту
		/// </summary>
		public class CounterpartySettlementsReconciliation
		{
			public static DateTime OldOrdersMaxDate = new DateTime(2020, 08, 12);

			private const int _datePositionIndex = 0;
			private const int _orderPaymentInfoPositionIndex = 1;
			private const int _orderSumPositionIndex = 3;
			private const int _paymentSumPositionIndex = 5;
			private const int _sosnovtsevInfoPositionIndex = 2;
			private const int _innPositionIndex = 9;
			private const int _periodSumsInfoPositionIndex = 0;
			private const int _periodTotalOrdersSumPositionIndex = 3;
			private const int _periodTotalPaymentsSumPositionIndex = 5;

			private readonly IList<IList<string>> _reconciliationRows;

			private CounterpartySettlementsReconciliation(IList<IList<string>> reconciliationRows)
			{
				_reconciliationRows = reconciliationRows ?? throw new ArgumentNullException(nameof(reconciliationRows));

				CounterpartyInn = GetCounterpartyInn();
				Orders = GetOrderReconciliations();
				Payments = GetPaymentReconciliations();

				OrdersTotalSum = GetCounterpartyOrdersTotalSum();
				PaymentsTotalSum = GetCounterpartyPaymentsTotalSum();
				CounterpartyOldBalance = GetCounterpartyOldBalance();
			}

			#region Properties
			public string CounterpartyInn { get; }
			public IList<OrderReconciliation> Orders { get; }
			public IList<PaymentReconciliation> Payments { get; }
			public decimal OrdersTotalSum { get; }
			public decimal PaymentsTotalSum { get; }
			public decimal CounterpartyTotalDebt => OrdersTotalSum - PaymentsTotalSum;
			public decimal CounterpartyOldBalance { get; }

			#endregion Properties

			public static CounterpartySettlementsReconciliation CreateFromXlsx(string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				var reconciliationOfMutualSettlements = Create(rowsFromXls);

				return reconciliationOfMutualSettlements;
			}

			private static CounterpartySettlementsReconciliation Create(IList<IList<string>> rows)
			{

				return new CounterpartySettlementsReconciliation(rows);
			}

			private string GetCounterpartyInn()
			{
				var counterpartyInn = string.Empty;

				foreach(var rowData in _reconciliationRows)
				{
					if(rowData.Count < _innPositionIndex + 1)
					{
						continue;
					}

					if(rowData.Count > 1 && rowData[_sosnovtsevInfoPositionIndex].StartsWith("(Сосновцев"))
					{
						counterpartyInn = XlsParseHelper.ParseClientInnFromString(rowData[_innPositionIndex]);
						continue;
					}
				}

				return counterpartyInn;
			}

			private IList<OrderReconciliation> GetOrderReconciliations()
			{
				var orderNodes = new List<OrderReconciliation>();

				foreach(var rowData in _reconciliationRows)
				{
					if(TryCreateOrderReconciliation(rowData, out OrderReconciliation order))
					{
						orderNodes.Add(order);
					}
				}

				return orderNodes;
			}

			private bool TryCreateOrderReconciliation(IList<string> rowData, out OrderReconciliation order)
			{
				order = null;

				if(!IsOrderOrPaymentDataRow(rowData))
				{
					return false;
				}

				if(!rowData[_orderPaymentInfoPositionIndex].StartsWith("Продажа"))
				{
					return false;
				}

				var orderId = XlsParseHelper.ParseNumberFromString(rowData[_orderPaymentInfoPositionIndex]);

				if(orderId == null)
				{
					return false;
				}

				order = new OrderReconciliation
				{
					OrderId = orderId.Value,
					OrderDeliveryDate = XlsParseHelper.ParseDateFromString(rowData[_orderPaymentInfoPositionIndex]),
					OrderSum = decimal.Parse(rowData[_orderSumPositionIndex])
				};

				return true;
			}

			private IList<PaymentReconciliation> GetPaymentReconciliations()
			{
				var payments = new List<PaymentReconciliation>();

				foreach(var rowData in _reconciliationRows)
				{
					if(TryCreatePaymentReconciliation(rowData, out PaymentReconciliation payment))
					{
						payments.Add(payment);
					}
				}

				return payments;
			}

			private bool TryCreatePaymentReconciliation(IList<string> rowData, out PaymentReconciliation payment)
			{
				payment = null;

				if(!IsOrderOrPaymentDataRow(rowData))
				{
					return false;
				}

				if(!rowData[_orderPaymentInfoPositionIndex].StartsWith("Оплата"))
				{
					return false;
				}

				var paymentNum = XlsParseHelper.ParseNumberFromString(rowData[_orderPaymentInfoPositionIndex]);
				var paymentDate = XlsParseHelper.ParseDateFromString(rowData[_orderPaymentInfoPositionIndex]);

				if(paymentNum is null || paymentDate is null)
				{
					return false;
				}

				payment = new PaymentReconciliation
				{
					PaymentNum = paymentNum.Value,
					PaymentDate = paymentDate.Value,
					PaymentSum = decimal.Parse(rowData[_paymentSumPositionIndex])
				};

				return true;
			}

			private bool IsOrderOrPaymentDataRow(IList<string> rowData)
			{
				if(rowData.Count < _paymentSumPositionIndex + 1)
				{
					return false;
				}

				DateTime.TryParse(rowData[_datePositionIndex], out var date);

				if(date == default)
				{
					return false;
				}

				return true;
			}

			private decimal GetCounterpartyOrdersTotalSum()
			{
				var sum = default(decimal);

				foreach(var rowData in _reconciliationRows)
				{
					if(rowData.Count < _periodTotalOrdersSumPositionIndex + 1)
					{
						continue;
					}

					if(rowData[_periodSumsInfoPositionIndex].StartsWith("Обороты за период"))
					{
						sum = decimal.Parse(rowData[_periodTotalOrdersSumPositionIndex]);
					}
				}

				return sum;
			}

			private decimal GetCounterpartyPaymentsTotalSum()
			{
				var sum = default(decimal);

				foreach(var rowData in _reconciliationRows)
				{
					if(rowData.Count < _periodTotalPaymentsSumPositionIndex + 1)
					{
						continue;
					}

					if(rowData[_periodSumsInfoPositionIndex].StartsWith("Обороты за период"))
					{
						sum = decimal.Parse(rowData[_periodTotalPaymentsSumPositionIndex]);
					}
				}

				return sum;
			}

			private decimal GetCounterpartyOldBalance()
			{
				var debt = default(decimal);

				foreach(var rowData in _reconciliationRows)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Продажа"))
					{
						debt -= decimal.Parse(rowData[_orderSumPositionIndex]);
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						debt += decimal.Parse(rowData[_paymentSumPositionIndex]);
					}
				}

				return debt;
			}

			public class OrderReconciliation
			{
				public int OrderId { get; set; }
				public DateTime? OrderDeliveryDate { get; set; }
				public decimal OrderSum { get; set; }

			}

			public class PaymentReconciliation
			{
				public int PaymentNum { get; set; }
				public DateTime PaymentDate { get; set; }
				public decimal PaymentSum { get; set; }
			}
		}
	}
}
