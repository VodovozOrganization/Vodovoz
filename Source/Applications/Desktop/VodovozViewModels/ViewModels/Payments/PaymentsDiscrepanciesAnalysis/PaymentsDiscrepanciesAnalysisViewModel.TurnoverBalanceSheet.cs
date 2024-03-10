using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Оборотно-сальдовая ведомость
		/// </summary>
		public class TurnoverBalanceSheet
		{
			private const int _innPositionIndex = 0;
			private const int _debetPositionIndex = 5;
			private const int _creditPositionIndex = 6;

			private readonly IList<IList<string>> _balanceRows;

			private TurnoverBalanceSheet(IList<IList<string>> balanceRows)
			{
				_balanceRows = balanceRows ?? throw new System.ArgumentNullException(nameof(balanceRows));

				CounterpartyBalances = GetCounterpartyBalances();
			}

			public IList<CounterpartyBalance> CounterpartyBalances { get; set; }

			public static TurnoverBalanceSheet CreateFromXls(string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				return new TurnoverBalanceSheet(rowsFromXls);
			}

			private IList<CounterpartyBalance> GetCounterpartyBalances()
			{
				var balanceNodes = new List<CounterpartyBalance>();

				foreach(var rowData in _balanceRows)
				{
					if(TryCreateCounterpartyBalance(rowData, out CounterpartyBalance balance))
					{
						balanceNodes.Add(balance);
					}
				}

				return balanceNodes;
			}

			private bool TryCreateCounterpartyBalance(IList<string> rowData, out CounterpartyBalance balance)
			{
				balance = null;

				if(rowData.Count < _creditPositionIndex + 1)
				{
					return false;
				}

				var inn = XlsParseHelper.ParseClientInnFromString(rowData[_innPositionIndex]);

				if(string.IsNullOrWhiteSpace(inn))
				{
					return false;
				}

				var debit = XlsParseHelper.ParseFloatingPointNumberFromString(rowData[_debetPositionIndex]);
				var credit = XlsParseHelper.ParseFloatingPointNumberFromString(rowData[_creditPositionIndex]);

				balance = new CounterpartyBalance
				{
					Inn = inn,
					Debit = debit,
					Credit = credit
				};

				return true;
			}

			public class CounterpartyBalance
			{
				public string Inn { get; set; }
				public decimal? Debit { get; set; }
				public decimal? Credit { get; set; }
			}
		}
	}
}
