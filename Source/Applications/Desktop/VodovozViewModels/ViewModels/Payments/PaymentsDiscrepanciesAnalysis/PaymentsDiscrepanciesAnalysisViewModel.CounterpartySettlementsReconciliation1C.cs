using System;
using System.Collections.Generic;
using System.Linq;

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

			private const string _openingBalancePrefix = "Сальдо начальное";
			private const string _closingBalancePrefix = "Сальдо конечное";
			private const string _periodTurnoverPrefix = "Обороты за период";
			private const string _salePrefix = "Продажа";
			private const string _paymentPrefix = "Оплата";
			private const string _paymentOrderPrefix = "Платежное поручение";
			private const string _dateHeader = "Дата";
			private const string _documentHeader = "Документ";
			private const string _debitHeader = "Дебет";
			private const string _creditHeader = "Кредит";

			private readonly IList<IList<string>> _reconciliationRows;
			private readonly IList<ReconciliationMovement> _movements;
			private readonly IList<TurnoverSummary> _turnoverSummaries;

			private CounterpartySettlementsReconciliation1C(IList<IList<string>> reconciliationRows)
			{
				_reconciliationRows = reconciliationRows ?? throw new ArgumentNullException(nameof(reconciliationRows));
				_movements = GetReconciliationMovements(out _turnoverSummaries);

				CounterpartyInn = GetCounterpartyInn();
				Orders = GetOrderReconciliations();
				OtherWriteOffs = GetOtherWriteOffReconciliations();
				Payments = GetPaymentReconciliations();
				OtherIncomes = GetOtherIncomeReconciliations();

				OrdersTotalSum = GetCounterpartyOrdersTotalSum();
				PaymentsTotalSum = GetCounterpartyPaymentsTotalSum();
				CounterpartyOldBalance = GetCounterpartyOldBalance();
			}

			#region Properties

			public string CounterpartyInn { get; }
			public IList<OrderReconciliation1C> Orders { get; }
			public IList<OtherWriteOffReconciliation1C> OtherWriteOffs { get; }
			public IList<PaymentReconciliation1C> Payments { get; }
			public IList<OtherIncomeReconciliation1C> OtherIncomes { get; }
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
				foreach(var rowData in _reconciliationRows)
				{
					if(rowData.Any(cell => cell.StartsWith("(Сосновцев")))
					{
						return rowData
							.Select(XlsParseHelper.ParseClientInnFromString)
							.FirstOrDefault(inn => !string.IsNullOrWhiteSpace(inn));
					}
				}

				foreach(var rowData in _reconciliationRows.Where(row => row.Any(cell => cell.Contains("ИНН"))))
				{
					var counterpartyInn = rowData
						.Select(XlsParseHelper.ParseClientInnFromString)
						.FirstOrDefault(inn => !string.IsNullOrWhiteSpace(inn));

					if(!string.IsNullOrWhiteSpace(counterpartyInn))
					{
						return counterpartyInn;
					}
				}

				return _reconciliationRows
					.SelectMany(row => row)
					.Select(XlsParseHelper.ParseClientInnFromString)
					.FirstOrDefault(inn => !string.IsNullOrWhiteSpace(inn)) ?? string.Empty;
			}

			private IList<OrderReconciliation1C> GetOrderReconciliations()
			{
				var orderNodes = new List<OrderReconciliation1C>();
				foreach(var movement in _movements)
				{
					if(TryCreateOrderReconciliation(movement, out var order))
					{
						orderNodes.Add(order);
					}
				}

				return orderNodes;
			}

			private bool TryCreateOrderReconciliation(ReconciliationMovement movement, out OrderReconciliation1C order)
			{
				order = null;

				if(!movement.Debit.HasValue || !IsSaleDocument(movement.DocumentName))
				{
					return false;
				}

				var orderId = XlsParseHelper.ParseNumberFromString(movement.DocumentName);

				if(!orderId.HasValue)
				{
					return false;
				}

				order = new OrderReconciliation1C
				{
					OrderId = orderId.Value,
					OrderDeliveryDate = movement.DocumentDate ?? movement.Date,
					OrderSum = movement.Debit.Value,
					DocumentName = movement.DocumentName,
					IsRecognizedOrder = true
				};

				return true;
			}

			private IList<OtherWriteOffReconciliation1C> GetOtherWriteOffReconciliations()
			{
				var writeOffs = new List<OtherWriteOffReconciliation1C>();

				foreach(var movement in _movements)
				{
					if(TryCreateOtherWriteOffReconciliation(movement, out var writeOff))
					{
						writeOffs.Add(writeOff);
					}
				}

				return writeOffs;
			}

			private bool TryCreateOtherWriteOffReconciliation(
				ReconciliationMovement movement,
				out OtherWriteOffReconciliation1C writeOff)
			{
				writeOff = null;

				if(!movement.Debit.HasValue || IsSaleDocument(movement.DocumentName))
				{
					return false;
				}

				writeOff = new OtherWriteOffReconciliation1C
				{
					DocumentName = movement.DocumentName,
					DocumentNumber = movement.DocumentNumber,
					DocumentDate = movement.DocumentDate ?? movement.Date,
					WriteOffSum = movement.Debit.Value
				};

				return true;
			}

			private IList<PaymentReconciliation1C> GetPaymentReconciliations()
			{
				var payments = new List<PaymentReconciliation1C>();

				foreach(var movement in _movements)
				{
					if(TryCreatePaymentReconciliation(movement, out var payment))
					{
						payments.Add(payment);
					}
				}

				return payments;
			}

			private bool TryCreatePaymentReconciliation(ReconciliationMovement movement, out PaymentReconciliation1C payment)
			{
				payment = null;

				if(!movement.Credit.HasValue || !IsPaymentDocument(movement.DocumentName))
				{
					return false;
				}

				if(!movement.DocumentNumber.HasValue || !movement.DocumentDate.HasValue)
				{
					return false;
				}

				payment = new PaymentReconciliation1C
				{
					PaymentNum = movement.DocumentNumber.Value,
					PaymentDate = movement.DocumentDate.Value,
					PaymentSum = movement.Credit.Value
				};

				return true;
			}

			private IList<OtherIncomeReconciliation1C> GetOtherIncomeReconciliations()
			{
				var incomes = new List<OtherIncomeReconciliation1C>();

				foreach(var movement in _movements)
				{
					if(TryCreateOtherIncomeReconciliation(movement, out var income))
					{
						incomes.Add(income);
					}
				}

				return incomes;
			}

			private bool TryCreateOtherIncomeReconciliation(ReconciliationMovement movement, out OtherIncomeReconciliation1C income)
			{
				income = null;

				if(!movement.Credit.HasValue)
				{
					return false;
				}

				if(IsPaymentDocument(movement.DocumentName)
					&& movement.DocumentNumber.HasValue
					&& movement.DocumentDate.HasValue)
				{
					return false;
				}

				income = new OtherIncomeReconciliation1C
				{
					DocumentName = movement.DocumentName,
					DocumentNumber = movement.DocumentNumber,
					DocumentDate = movement.DocumentDate ?? movement.Date,
					IncomeSum = movement.Credit.Value
				};

				return true;
			}

			private IList<ReconciliationMovement> GetReconciliationMovements(out IList<TurnoverSummary> turnoverSummaries)
			{
				var movements = new List<ReconciliationMovement>();
				var summaries = new List<TurnoverSummary>();
				var isInsideTurnover = false;
				MovementLayout layout = null;

				for(var rowIndex = 0; rowIndex < _reconciliationRows.Count; rowIndex++)
				{
					var rowData = _reconciliationRows[rowIndex];

					if(RowContainsStartsWith(rowData, _openingBalancePrefix))
					{
						layout = GetMovementLayout(rowIndex);
						isInsideTurnover = layout.IsValid;
						continue;
					}

					if(!isInsideTurnover)
					{
						continue;
					}

					if(RowContainsStartsWith(rowData, _closingBalancePrefix))
					{
						isInsideTurnover = false;
						layout = null;
						continue;
					}

					if(RowContainsStartsWith(rowData, _periodTurnoverPrefix))
					{
						summaries.Add(new TurnoverSummary
						{
							Debit = GetAmount(rowData, layout.DebitIndex) ?? 0,
							Credit = GetAmount(rowData, layout.CreditIndex) ?? 0
						});

						continue;
					}

					if(TryCreateMovement(rowData, layout, out var movement))
					{
						movements.Add(movement);
					}
				}

				turnoverSummaries = summaries;
				return movements;
			}

			private MovementLayout GetMovementLayout(int openingBalanceRowIndex)
			{
				const int maxHeaderRowsLookBack = 8;

				var firstHeaderRowIndex = Math.Max(0, openingBalanceRowIndex - maxHeaderRowsLookBack);
				var headerRows = _reconciliationRows
					.Skip(firstHeaderRowIndex)
					.Take(openingBalanceRowIndex - firstHeaderRowIndex)
					.ToList();

				var dateIndex = FindCellIndex(headerRows, _dateHeader);
				var documentIndex = FindCellIndex(headerRows, _documentHeader);
				var debitIndexes = FindCellIndexes(headerRows, _debitHeader);
				var creditIndexes = FindCellIndexes(headerRows, _creditHeader);
				var debitIndex = debitIndexes.FirstOrDefault(index => index > documentIndex);
				var creditIndex = creditIndexes.FirstOrDefault(index => index > debitIndex);

				return new MovementLayout
				{
					DateIndex = dateIndex,
					DocumentIndex = documentIndex,
					DebitIndex = debitIndex == default && !debitIndexes.Contains(default) ? -1 : debitIndex,
					CreditIndex = creditIndex == default && !creditIndexes.Contains(default) ? -1 : creditIndex
				};
			}

			private int FindCellIndex(IList<IList<string>> rows, string value)
			{
				var indexes = FindCellIndexes(rows, value);

				return indexes.Any() ? indexes.First() : -1;
			}

			private IList<int> FindCellIndexes(IList<IList<string>> rows, string value)
			{
				var indexes = new List<int>();

				foreach(var row in rows)
				{
					for(var index = 0; index < row.Count; index++)
					{
						if(IsCellEqual(row[index], value) && !indexes.Contains(index))
						{
							indexes.Add(index);
						}
					}
				}

				return indexes;
			}

			private bool TryCreateMovement(
				IList<string> rowData,
				MovementLayout layout,
				out ReconciliationMovement movement)
			{
				movement = null;

				var rowDate = ParseDate(GetCell(rowData, layout.DateIndex));
				if(!rowDate.HasValue)
				{
					return false;
				}

				var documentName = GetCell(rowData, layout.DocumentIndex).Trim();
				if(string.IsNullOrWhiteSpace(documentName))
				{
					return false;
				}

				var debit = GetAmount(rowData, layout.DebitIndex);
				var credit = GetAmount(rowData, layout.CreditIndex);

				if(!debit.HasValue && !credit.HasValue)
				{
					return false;
				}

				movement = new ReconciliationMovement
				{
					Date = rowDate.Value,
					DocumentName = documentName,
					DocumentNumber = GetDocumentNumber(documentName),
					DocumentDate = XlsParseHelper.ParseDateFromString(documentName),
					Debit = debit,
					Credit = credit
				};

				return true;
			}

			private decimal? GetAmount(IList<string> rowData, int positionIndex)
			{
				if(positionIndex < 0 || rowData.Count < positionIndex + 1)
				{
					return null;
				}

				return XlsParseHelper.ParseFloatingPointNumberFromString(rowData[positionIndex]);
			}

			private string GetCell(IList<string> rowData, int positionIndex)
			{
				return positionIndex >= 0 && rowData.Count > positionIndex
					? rowData[positionIndex] ?? string.Empty
					: string.Empty;
			}

			private bool RowContainsStartsWith(IList<string> rowData, string prefix)
			{
				return rowData.Any(cell => (cell ?? string.Empty).Trim().StartsWith(prefix));
			}

			private bool IsCellEqual(string cellValue, string expectedValue)
			{
				return string.Equals((cellValue ?? string.Empty).Trim(), expectedValue, StringComparison.OrdinalIgnoreCase);
			}

			private bool IsSaleDocument(string documentName)
			{
				return (documentName ?? string.Empty).Trim().StartsWith(_salePrefix, StringComparison.OrdinalIgnoreCase);
			}

			private bool IsPaymentDocument(string documentName)
			{
				var normalizedDocumentName = (documentName ?? string.Empty).Trim();

				return normalizedDocumentName.StartsWith(_paymentPrefix, StringComparison.OrdinalIgnoreCase)
					|| normalizedDocumentName.StartsWith(_paymentOrderPrefix, StringComparison.OrdinalIgnoreCase);
			}

			private int? GetDocumentNumber(string documentName)
			{
				if(string.IsNullOrWhiteSpace(documentName))
				{
					return null;
				}

				if(!IsSaleDocument(documentName) && !IsPaymentDocument(documentName))
				{
					return null;
				}

				return XlsParseHelper.ParseNumberFromString(documentName);
			}

			private DateTime? ParseDate(string value)
			{
				return DateTime.TryParse(value, out var date) && date != default
					? date
					: (DateTime?)null;
			}

			private decimal GetCounterpartyOrdersTotalSum()
			{
				if(_turnoverSummaries.Any())
				{
					return _turnoverSummaries.Sum(summary => summary.Debit);
				}

				return _movements.Sum(movement => movement.Debit ?? 0);
			}

			private decimal GetCounterpartyPaymentsTotalSum()
			{
				if(_turnoverSummaries.Any())
				{
					return _turnoverSummaries.Sum(summary => summary.Credit);
				}

				return _movements.Sum(movement => movement.Credit ?? 0);
			}

			private decimal GetCounterpartyOldBalance()
			{
				var debt = default(decimal);

				foreach(var movement in _movements)
				{
					if(movement.Date >= OldOrdersMaxDate.AddDays(1))
					{
						continue;
					}

					if(movement.Debit.HasValue)
					{
						debt -= movement.Debit.Value;
					}

					if(movement.Credit.HasValue)
					{
						debt += movement.Credit.Value;
					}
				}

				return debt;
			}

			private class MovementLayout
			{
				public int DateIndex { get; set; } = -1;
				public int DocumentIndex { get; set; } = -1;
				public int DebitIndex { get; set; } = -1;
				public int CreditIndex { get; set; } = -1;

				public bool IsValid =>
					DateIndex >= 0
					&& DocumentIndex >= 0
					&& DebitIndex >= 0
					&& CreditIndex >= 0;
			}

			private class ReconciliationMovement
			{
				public DateTime Date { get; set; }
				public string DocumentName { get; set; }
				public int? DocumentNumber { get; set; }
				public DateTime? DocumentDate { get; set; }
				public decimal? Debit { get; set; }
				public decimal? Credit { get; set; }
			}

			private class TurnoverSummary
			{
				public decimal Debit { get; set; }
				public decimal Credit { get; set; }
			}
		}
	}
}
