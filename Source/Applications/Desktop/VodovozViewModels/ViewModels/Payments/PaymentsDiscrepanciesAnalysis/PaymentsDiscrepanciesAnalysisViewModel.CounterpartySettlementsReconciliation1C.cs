using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Сверка взаиморасчетов по контрагенту 1С
		/// </summary>
		public class CounterpartySettlementsReconciliation1C
		{
			public static readonly DateTime OldOrdersMaxDate = new DateTime(2020, 08, 12);

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

			private CounterpartySettlementsReconciliation1C(IList<IList<string>> reconciliationRows)
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
			public IList<OrderReconciliation1C> Orders { get; }
			public IList<PaymentReconciliation1C> Payments { get; }
			public decimal OrdersTotalSum { get; }
			public decimal PaymentsTotalSum { get; }
			public decimal CounterpartyBalance => PaymentsTotalSum - OrdersTotalSum;
			public decimal CounterpartyOldBalance { get; }

			#endregion Properties

			public static CounterpartySettlementsReconciliation1C CreateFromXlsx(string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				var reconciliationOfMutualSettlements = Create(rowsFromXls);

				return reconciliationOfMutualSettlements;
			}

			private static CounterpartySettlementsReconciliation1C Create(IList<IList<string>> rows)
			{
				return new CounterpartySettlementsReconciliation1C(rows);
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

			private IList<OrderReconciliation1C> GetOrderReconciliations()
			{
				var orderNodes = new List<OrderReconciliation1C>();

				foreach(var rowData in _reconciliationRows)
				{
					if(TryCreateOrderReconciliation(rowData, out OrderReconciliation1C order))
					{
						orderNodes.Add(order);
					}
				}

				return orderNodes;
			}

			private bool TryCreateOrderReconciliation(IList<string> rowData, out OrderReconciliation1C order)
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

				order = new OrderReconciliation1C
				{
					OrderId = orderId.Value,
					OrderDeliveryDate = XlsParseHelper.ParseDateFromString(rowData[_orderPaymentInfoPositionIndex]),
					OrderSum = decimal.Parse(rowData[_orderSumPositionIndex], CultureInfo.InvariantCulture)
				};

				return true;
			}

			private IList<PaymentReconciliation1C> GetPaymentReconciliations()
			{
				var payments = new List<PaymentReconciliation1C>();

				foreach(var rowData in _reconciliationRows)
				{
					if(TryCreatePaymentReconciliation(rowData, out PaymentReconciliation1C payment))
					{
						payments.Add(payment);
					}
				}

				return payments;
			}

			private bool TryCreatePaymentReconciliation(IList<string> rowData, out PaymentReconciliation1C payment)
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

				payment = new PaymentReconciliation1C
				{
					PaymentNum = paymentNum.Value,
					PaymentDate = paymentDate.Value,
					PaymentSum = decimal.Parse(rowData[_paymentSumPositionIndex], CultureInfo.InvariantCulture)
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
						sum = decimal.Parse(rowData[_periodTotalOrdersSumPositionIndex], CultureInfo.InvariantCulture);
					}
				}

				return sum;
			}

			private decimal GetCounterpartyPaymentsTotalSum()
			{
				decimal sum = default;

				foreach(var rowData in _reconciliationRows)
				{
					if(rowData.Count < _periodTotalPaymentsSumPositionIndex + 1)
					{
						continue;
					}

					if(rowData[_periodSumsInfoPositionIndex].StartsWith("Обороты за период"))
					{
						sum = decimal.Parse(rowData[_periodTotalPaymentsSumPositionIndex], CultureInfo.InvariantCulture);
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

					DateTime.TryParse(rowData[_datePositionIndex], out var date);

					if(date == default || date >= OldOrdersMaxDate.AddDays(1))
					{
						continue;
					}

					if(rowData[1].StartsWith("Продажа"))
					{
						debt -= decimal.Parse(rowData[_orderSumPositionIndex], CultureInfo.InvariantCulture);
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						debt += decimal.Parse(rowData[_paymentSumPositionIndex], CultureInfo.InvariantCulture);
					}
				}

				return debt;
			}
		}
	}
}
