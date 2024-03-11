using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Оборотно-сальдовая ведомость 1С
		/// </summary>
		public class TurnoverBalanceSheet1C
		{
			private const int _innPositionIndex = 0;
			private const int _debetPositionIndex = 5;
			private const int _creditPositionIndex = 6;

			private readonly IList<IList<string>> _balanceRows;

			private TurnoverBalanceSheet1C(IList<IList<string>> balanceRows)
			{
				_balanceRows = balanceRows ?? throw new System.ArgumentNullException(nameof(balanceRows));

				CounterpartyBalances = GetCounterpartyBalances();
			}

			public IList<CounterpartyBalance1C> CounterpartyBalances { get; set; }

			public static TurnoverBalanceSheet1C CreateFromXlsx(string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				return new TurnoverBalanceSheet1C(rowsFromXls);
			}

			private IList<CounterpartyBalance1C> GetCounterpartyBalances()
			{
				var balanceNodes = new List<CounterpartyBalance1C>();

				foreach(var rowData in _balanceRows)
				{
					if(TryCreateCounterpartyBalance(rowData, out CounterpartyBalance1C balance))
					{
						balanceNodes.Add(balance);
					}
				}

				return balanceNodes;
			}

			private bool TryCreateCounterpartyBalance(IList<string> rowData, out CounterpartyBalance1C balance)
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

				balance = new CounterpartyBalance1C
				{
					Inn = inn,
					Debit = debit,
					Credit = credit
				};

				return true;
			}
		}
	}
}
