using System;
using System.Collections.Generic;
using System.Linq;

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
				CounterpartyOldDebt = GetCounterpartyOldDebt();
			}

			#region Properties
			public string CounterpartyInn { get; }
			public IList<OrderReconciliation> Orders { get; }
			public IList<PaymentReconciliation> Payments { get; }
			public decimal OrdersTotalSum { get; }
			public decimal PaymentsTotalSum { get; }
			public decimal CounterpartyTotalDebt => OrdersTotalSum - PaymentsTotalSum;
			public decimal CounterpartyOldDebt { get; }

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
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[_orderPaymentInfoPositionIndex].StartsWith("Продажа"))
					{
						var order = CreateOrderReconciliation(rowData);

						orderNodes.Add(order);
					}
				}

				return orderNodes;
			}

			private static OrderReconciliation CreateOrderReconciliation(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var order = new OrderReconciliation
				{
					OrderId = XlsParseHelper.ParseNumberFromString(data[_orderPaymentInfoPositionIndex]),
					OrderDeliveryDate = XlsParseHelper.ParseDateFromString(data[_orderPaymentInfoPositionIndex]),
					OrderSum = decimal.Parse(data[_orderSumPositionIndex])
				};

				return order;
			}

			private IList<PaymentReconciliation> GetPaymentReconciliations()
			{
				var payments = new List<PaymentReconciliation>();

				foreach(var rowData in _reconciliationRows)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[_orderPaymentInfoPositionIndex].StartsWith("Оплата"))
					{
						var payment = CreatePaymentReconciliation(rowData);

						payments.Add(payment);
					}
				}

				return payments;
			}

			private static PaymentReconciliation CreatePaymentReconciliation(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var payment = new PaymentReconciliation
				{
					PaymentNum = XlsParseHelper.ParseNumberFromString(data[_orderPaymentInfoPositionIndex]),
					PaymentDate = XlsParseHelper.ParseDateFromString(data[_orderPaymentInfoPositionIndex]),
					PaymentSum = decimal.Parse(data[_paymentSumPositionIndex])
				};

				return payment;
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

			private decimal GetCounterpartyOldDebt()
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
						debt += decimal.Parse(rowData[_orderSumPositionIndex]);
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						debt -= decimal.Parse(rowData[_paymentSumPositionIndex]);
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
