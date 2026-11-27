using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.Cash
{
	/// <summary>
	/// Распределение возвратов по известным центрам закрытых авансов
	/// </summary>
	public static class CashFlowReportReturnAllocation
	{
		/// <summary>
		/// Преобразует каждый приходный возврат в уменьшение расходов
		/// </summary>
		/// <param name="returns">Приходные ордера возврата</param>
		/// <param name="directClosings">Закрытия авансов приходными ордерами</param>
		/// <param name="advanceReportClosings">Закрытия авансов отчётами с возвратом сдачи</param>
		/// <returns>Суммы уменьшения расходов по статьям и центрам</returns>
		public static IEnumerable<CashFlowReportExpenseNode> Allocate(
			IEnumerable<CashFlowReportReturnNode> returns,
			IEnumerable<CashFlowReportAdvanceClosingNode> directClosings,
			IEnumerable<CashFlowReportAdvanceClosingNode> advanceReportClosings)
		{
			var directByReturn = directClosings.ToLookup(x => x.ReturnId);
			var indirectByReturn = advanceReportClosings.ToLookup(x => x.ReturnId);

			foreach(var cashReturn in returns)
			{
				var direct = directByReturn[cashReturn.Id];

				if(direct.Any() && direct.Sum(x => x.Money) == cashReturn.Money)
				{
					foreach(var closing in direct)
					{
						yield return CreateExpense(cashReturn.ExpenseCategoryId, closing.CfoSubdivisionId, closing.CfzSubdivisionId, closing.Money);
					}

					continue;
				}

				var related = direct.Concat(indirectByReturn[cashReturn.Id]);

				yield return CreateExpense(
					cashReturn.ExpenseCategoryId,
					GetCommonCenter(related.Select(x => x.CfoSubdivisionId)),
					GetCommonCenter(related.Select(x => x.CfzSubdivisionId)),
					cashReturn.Money);
			}
		}

		private static int? GetCommonCenter(IEnumerable<int?> centers)
		{
			var possibleCenters = centers.Distinct().Take(2).ToArray();
			return possibleCenters.Length == 1 ? possibleCenters[0] : null;
		}

		private static CashFlowReportExpenseNode CreateExpense(
			int expenseCategoryId, int? cfoSubdivisionId, int? cfzSubdivisionId, decimal returnMoney)
		{
			return new CashFlowReportExpenseNode
			{
				ExpenseCategoryId = expenseCategoryId,
				CfoSubdivisionId = cfoSubdivisionId,
				CfzSubdivisionId = cfzSubdivisionId,
				Money = -returnMoney
			};
		}
	}
}
