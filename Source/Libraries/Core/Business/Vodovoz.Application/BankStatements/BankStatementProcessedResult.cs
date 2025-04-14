using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vodovoz.Extensions;

namespace Vodovoz.Application.BankStatements
{
	/// <summary>
	/// Результаты обработки банковских выписок
	/// </summary>
	public class BankStatementProcessedResult
	{
		private readonly Dictionary<string, (decimal Balance, DateTime Date)> _bankStatementData
			= new Dictionary<string, (decimal Balance, DateTime Date)>();
		private readonly IEnumerable<(string FileName, BankStatementProcessState StateType)> _results
			= new List<(string FileName, BankStatementProcessState StateType)>();
		
		public IReadOnlyDictionary<string, (decimal Balance, DateTime Date)> BankStatementData => _bankStatementData;
		
		public void AddSuccessResult(string filePath, string accountNumber, decimal balance, DateTime date)
		{
			if(_bankStatementData.ContainsKey(accountNumber))
			{
				AddResult(filePath, BankStatementProcessState.BankStatementDuplicate);
				return;
			}
			
			_bankStatementData.Add(accountNumber, (balance, date));
			AddResult(filePath, BankStatementProcessState.Success);
		}
		
		public void AddResult(string filePath, BankStatementProcessState stateType)
		{
			var fileName = Path.GetFileName(filePath);
			((IList)_results).Add((fileName, stateType));
		}

		public string GetResult()
		{
			var sb = new StringBuilder();

			var filesCount = _results.Count();
			sb.AppendLine($"Обработка файлов завершена. Всего обработано файлов: {filesCount}шт");

			if(_results.Any(x => x.StateType != BankStatementProcessState.Success))
			{
				var errors = _results
					.Where(x => x.StateType != BankStatementProcessState.Success)
					.ToLookup(x => x.StateType);

				var errorsCount = errors.Sum(x => x.Count());
				sb.AppendLine($"Из них: успешно - {filesCount - errorsCount}шт, с ошибками - {errorsCount}шт");
				sb.AppendLine();
				var i = 0;

				foreach(var resultsGroup in errors)
				{
					i++;
					sb.AppendLine($"{i}. {resultsGroup.Key.GetEnumDisplayName()} - {resultsGroup.Count()}шт");
					sb.Append("Файлы: ");
					
					foreach(var result in resultsGroup)
					{
						sb.Append($"{result.FileName}, ");
					}
					
					sb.Remove(sb.Length - 2, 2)
						.AppendLine()
						.AppendLine();
				}
			}

			return sb.ToString();
		}
	}
}
