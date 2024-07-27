using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace Vodovoz.Application.BankStatements
{
	public class BankStatementHandler
	{
		private const string _accountNumber = "номер счета";
		private const string _endDate = "конечная дата";
		private const string _accountPattern = @"\D*[с|c]чет\D*";
		private const string _accountNumberPattern = @"[с|c]чет\D+([0-9]{20,25})";
		private const string _accountNumberWithDatePattern =
			@"[с|c]чет\D+([0-9]{20,25})\sза\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})\s-\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		private const string _balanceWithDatePattern = @"([0-9]{1,}[,|\.][0-9]{1,2})\D+([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		private const string _balancePattern = @"([0-9]{1,}[,|\.]*[0-9]*)";
		private const string _singleDatePattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		//с 15.07.2024 по 19.07.2024
		private const string _dateNumberPattern = @"[Сc|Cс]\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})|\D+([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		//с 15 июля 2024 по 19 июля 2024
		private const string _dateStringMonthPattern = @"[Сc|Cс]\s([0-9]{2}\D+[0-9]{4})\D+([0-9]{2}\D+[0-9]{4})";

		private readonly ILogger<BankStatementHandler> _logger;
		private readonly BankStatementParser _parser;
		
		public BankStatementHandler(
			ILogger<BankStatementHandler> logger,
			BankStatementParser parser)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_parser = parser ?? throw new ArgumentNullException(nameof(parser));
		}
		
		public BankStatementProcessedResult ProcessBankStatementsFromFile(string filePath, DateTime date)
		{
			var bankStatementProcessedResult = new BankStatementProcessedResult();
			
			TryParseData(filePath, date, bankStatementProcessedResult);

			return bankStatementProcessedResult;
		}
		
		public BankStatementProcessedResult ProcessBankStatementsFromDirectory(string directoryPath, DateTime date)
		{
			var bankStatementProcessedResult = new BankStatementProcessedResult();
			
			var filesPaths = Directory.GetFiles(directoryPath);

			if(!filesPaths.Any())
			{
				bankStatementProcessedResult.AddResult(null, BankStatementProcessState.EmptyDirectory);
				return bankStatementProcessedResult;
			}
			
			foreach(var filePath in filesPaths)
			{
				TryParseData(filePath, date, bankStatementProcessedResult);
			}

			return bankStatementProcessedResult;
		}

		private void TryParseData(string filePath, DateTime date, BankStatementProcessedResult bankStatementProcessedResult)
		{
			var fileExtension = CheckFileExtension(filePath);
			if(fileExtension is null)
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.UnsupportedFileExtension);
				return;
			}
			
			TryParseData(filePath, fileExtension.Value, date, bankStatementProcessedResult);
		}

		private void TryParseData(
			string filePath,
			BankStatementFileExtension fileExtension,
			DateTime date,
			BankStatementProcessedResult bankStatementProcessedResult)
		{
			try
			{
				var parsedData = _parser.TryParse(filePath, fileExtension);
				var result = TryProcessData(parsedData);
				
				if(CheckWrongData(filePath, date, bankStatementProcessedResult, result))
				{
					return;
				}

				bankStatementProcessedResult.AddSuccessResult(filePath, result.AccountNumber, result.Balance.Value, result.Date.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при парсинге файла выписки {FilePath}", filePath);
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.ErrorParsingFile);
			}
		}

		private bool CheckWrongData(
			string filePath,
			DateTime date,
			BankStatementProcessedResult bankStatementProcessedResult,
			(string AccountNumber, decimal? Balance, DateTime? Date) result)
		{
			if(string.IsNullOrWhiteSpace(result.AccountNumber))
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.EmptyAccountNumber);
				return true;
			}

			if(!result.Balance.HasValue)
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.EmptyBalance);
				return true;
			}

			if(!result.Date.HasValue)
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.EmptyDate);
				return true;
			}
				
			if(result.Date != date)
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.WrongBankStatementDate);
				return true;
			}

			return false;
		}

		private BankStatementFileExtension? CheckFileExtension(string fileName)
		{
			var extension = Path.GetExtension(fileName).ToLower();
			if(Enum.TryParse(extension.Substring(1), out BankStatementFileExtension fileExtension))
			{
				return fileExtension;
			}
			return null;
		}
		
		private (string AccountNumber, decimal? Balance, DateTime? Date) TryProcessData(IEnumerable<IEnumerable<string>> parsedData)
		{
			string accountNumber = null;
			string balance = null;
			string date = null;

			TryProcessAccountNumberAndDate(parsedData, ref accountNumber, ref date);

			if(string.IsNullOrWhiteSpace(accountNumber))
			{
				TryProcessAccountNumber(parsedData, ref accountNumber);
			}

			if(string.IsNullOrWhiteSpace(date))
			{
				TryProcessDateRow(parsedData, ref date);
			}
			
			TryProcessBalanceRow(parsedData, ref balance);

			decimal? convertedBalance = TryParseBalance(balance);
			DateTime? convertedDate = TryParseDate(date);

			return (accountNumber, convertedBalance, convertedDate);
		}
		
		private void TryProcessAccountNumberAndDate(
			IEnumerable<IEnumerable<string>> parsedData, ref string accountNumber, ref string date)
		{
			foreach(var row in parsedData)
			{
				if(accountNumber != null && date != null)
				{
					break;
				}
				
				foreach(var cell in row)
				{
					if(string.IsNullOrWhiteSpace(accountNumber))
					{
						var accountMatches = Regex.Matches(cell.ToLower(), _accountNumberPattern);
						var accountWithDateMatches = Regex.Matches(cell.ToLower(), _accountNumberWithDatePattern);

						if(accountMatches.Count != 0)
						{
							accountNumber = accountMatches[accountMatches.Count - 1].Groups[1].Value;
						}
						
						if(accountWithDateMatches.Count != 0)
						{
							accountNumber = accountWithDateMatches[accountWithDateMatches.Count - 1].Groups[1].Value;
							date = accountWithDateMatches[accountWithDateMatches.Count - 1].Groups[3].Value;
						}
					}

					if(string.IsNullOrWhiteSpace(date))
					{
						var dateNumberMatches = Regex.Matches(cell, _dateNumberPattern);
						var dateStringMonthMatches = Regex.Matches(cell, _dateStringMonthPattern);

						if(dateNumberMatches.Count != 0)
						{
							date = dateNumberMatches[dateNumberMatches.Count - 1].Groups[2].Value;
						}
						
						if(dateStringMonthMatches.Count != 0)
						{
							date = dateStringMonthMatches[dateStringMonthMatches.Count - 1].Groups[2].Value;
						}
					}

					if(accountNumber != null && date != null)
					{
						break;
					}
				}
			}
		}
		
		private void TryProcessAccountNumber(IEnumerable<IEnumerable<string>> parsedData, ref string accountNumber)
		{
			var success = false;
			var result = string.Empty;
			
			foreach(var row in parsedData)
			{
				if(success)
				{
					break;
				}
				
				if(row.Count() <= 1)
				{
					continue;
				}
				
				foreach(var cell in row)
				{
					var accountMatches = Regex.Matches(cell.ToLower(), _accountPattern);

					if(accountMatches.Count != 0)
					{
						success = true;
					}

					if(success)
					{
						result += $" {cell}";
					}
				}
			}
			
			var accountNumberMatches = Regex.Matches(result.ToLower(), _accountNumberPattern);

			if(accountNumberMatches.Count != 0)
			{
				accountNumber = accountNumberMatches[accountNumberMatches.Count - 1].Groups[1].Value;
			}
		}
		
		private void TryProcessDateRow(IEnumerable<IEnumerable<string>> parsedData, ref string date)
		{
			var dateSearchPattern = Enum.GetValues(typeof(BankStatementDateType));
			
			foreach(BankStatementDateType searchPattern in dateSearchPattern)
			{
				if(!string.IsNullOrWhiteSpace(date))
				{
					break;
				}
				
				var dateRow = GetDateRow(parsedData, searchPattern);
			
				if(dateRow.DateType is null) continue;

				switch(dateRow.DateType)
				{
					case BankStatementDateType.FromDate:
						TryProcessPeriodDate(dateRow.Data, ref date);
						break;
					case BankStatementDateType.OnDate:
						TryProcessSingleDate(dateRow.Data, ref date);
						break;
					case BankStatementDateType.EndDate:
						TryProcessSingleDate(dateRow.Data, ref date);
						break;
				}
			}
		}

		private void TryProcessPeriodDate(string dateRow, ref string date)
		{
			var dateMatches = Regex.Matches(dateRow, _dateNumberPattern);

			if(dateMatches.Count != 0)
			{
				date = dateMatches[dateMatches.Count - 1].Groups[2].Value;
			}
		}

		private void TryProcessSingleDate(string dateRow, ref string date)
		{
			var dateMatches = Regex.Matches(dateRow, _singleDatePattern);

			if(dateMatches.Count != 0)
			{
				date = dateMatches[dateMatches.Count - 1].Groups[1].Value;
			}

			if(!string.IsNullOrWhiteSpace(date))
			{
				return;
			}
			
			if(dateMatches.Count != 0)
			{
				date = dateMatches[dateMatches.Count - 1].Groups[2].Value;
			}
		}

		private (BankStatementDateType? DateType,string Data) GetDateRow(
			IEnumerable<IEnumerable<string>> parsedData, BankStatementDateType searchPattern)
		{
			var success = false;
			var result = string.Empty;
			
			foreach(var row in parsedData)
			{
				if(success)
				{
					break;
				}
				
				foreach(var cell in row)
				{
					switch(searchPattern)
					{
						case BankStatementDateType.EndDate:
							if(cell.ToLower().StartsWith(searchPattern.GetEnumDisplayName()))
							{
								success = true;
							}
							break;
						default:
							if(cell.ToLower() == searchPattern.GetEnumDisplayName())
							{
								success = true;
							}
							break;
					}
					
					if(success)
					{
						result += $" {cell}";
					}
				}
			}

			if(success)
			{
				return (searchPattern, result);
			}

			return (null, result);
		}

		private void TryProcessBalanceRow(IEnumerable<IEnumerable<string>> parsedData, ref string balance)
		{
			var balanceRow = GetBalanceOutRow(parsedData);
			
			if(balanceRow.BalanceType is null) return;

			switch(balanceRow.BalanceType)
			{
				case BankStatementBalanceType.OutgoingBalance:
					TryProcessOutgoingBalance(balanceRow.Data, ref balance);
					break;
				case BankStatementBalanceType.OutSaldo:
					TryProcessOutSaldo(balanceRow.Data, ref balance);
					break;
				case BankStatementBalanceType.OutgoingSaldo:
					TryProcessOutgoingSaldo(balanceRow.Data, ref balance);
					break;
			}
		}

		private (BankStatementBalanceType? BalanceType, IEnumerable<string> Data) GetBalanceOutRow(IEnumerable<IEnumerable<string>> parsedData)
		{
			var list = new List<string>();
			(bool Success, BankStatementBalanceType? BalanceType) result = (false, null);

			var balanceTypes = Enum.GetValues(typeof(BankStatementBalanceType));

			foreach(BankStatementBalanceType balanceType in balanceTypes)
			{
				if(result.Success)
				{
					break;
				}
				
				result = TryFindBalanceRow(parsedData, balanceType, list);
			}

			if(result.Success)
			{
				return (result.BalanceType, list);
			}

			return (null, list);
		}

		private (bool Success, BankStatementBalanceType? BalanceType) TryFindBalanceRow(
			IEnumerable<IEnumerable<string>> parsedData, BankStatementBalanceType balanceType, List<string> list)
		{
			var success = false;
			
			foreach(var row in parsedData)
			{
				foreach(var cell in row)
				{
					if(cell.ToLower().Contains(balanceType.GetEnumDisplayName()))
					{
						success = true;
					}

					if(success)
					{
						list.Add(cell);
					}
				}

				if(success)
				{
					break;
				}
			}

			if(success)
			{
				return (true, balanceType);
			}
			
			return (false, null);
		}

		private void TryProcessOutgoingBalance(IEnumerable<string> data, ref string balance)
		{
			foreach(var cell in data)
			{
				var balanceOnDateMatches = Regex.Matches(cell, _balanceWithDatePattern);
				var balanceMatches = Regex.Matches(cell, _balancePattern);

				if(balanceOnDateMatches.Count != 0)
				{
					balance = balanceOnDateMatches[balanceOnDateMatches.Count - 1].Groups[1].Value;
					break;
				}

				if(balanceMatches.Count != 0)
				{
					balance = balanceMatches[balanceMatches.Count - 1].Groups[1].Value;
					break;
				}
			}
		}
		
		private void TryProcessOutSaldo(IEnumerable<string> data, ref string balance)
		{
			foreach(var cell in data)
			{
				if(string.IsNullOrWhiteSpace(balance))
				{
					var balanceMatches = Regex.Matches(cell, _balancePattern);

					if(balanceMatches.Count != 0)
					{
						balance = balanceMatches[balanceMatches.Count - 1].Groups[1].Value;
					}
				}

				if(cell == "Д")
				{
					balance = $"-{balance}";
				}
			}
		}
		
		private void TryProcessOutgoingSaldo(IEnumerable<string> data, ref string balance)
		{
			foreach(var cell in data)
			{
				if(string.IsNullOrWhiteSpace(balance))
				{
					var balanceMatches = Regex.Matches(cell, _balancePattern);

					if(balanceMatches.Count != 0)
					{
						var balanceDebit = balanceMatches[balanceMatches.Count - 1].Groups[1].Value;
						var balanceCredit = balanceMatches[balanceMatches.Count - 1].Groups[2].Value;

						balance = balanceDebit == "0" ? balanceCredit : $"-{balanceDebit}";
					}
				}
			}
		}

		private decimal? TryParseBalance(string balance)
		{
			if(string.IsNullOrWhiteSpace(balance))
			{
				return null;
			}

			if(decimal.TryParse(balance, NumberStyles.Any, CultureInfo.InvariantCulture, out var convertedBalance))
			{
				return convertedBalance;
			}

			return null;
		}

		private DateTime? TryParseDate(string date)
		{
			if(string.IsNullOrWhiteSpace(date))
			{
				return null;
			}

			if(DateTime.TryParse(date, out var convertedDate))
			{
				return convertedDate;
			}

			return null;
		}
	}

	public class BankStatementProcessedResult
	{
		private readonly Dictionary<string, (decimal Balance, DateTime Date)> _bankStatementData
			= new Dictionary<string, (decimal Balance, DateTime Date)>();

		public IReadOnlyDictionary<string, (decimal Balance, DateTime Date)> BankStatementData => _bankStatementData;
		private IEnumerable<(string FileName, BankStatementProcessState StateType)> Results
			= new List<(string FileName, BankStatementProcessState StateType)>();
		
		public void AddSuccessResult(string filePath, string accountNumber, decimal balance, DateTime date)
		{
			_bankStatementData.Add(accountNumber, (balance, date));
			AddResult(filePath, BankStatementProcessState.Success);
		}
		
		public void AddResult(string filePath, BankStatementProcessState stateType)
		{
			var fileName = Path.GetFileName(filePath);
			((IList)Results).Add((fileName, stateType));
		}

		public string GetResult()
		{
			var sb = new StringBuilder();

			var filesCount = Results.Count();
			sb.AppendLine($"Обработка файлов завершена. Всего обработано файлов: {filesCount}шт");

			if(Results.Any(x => x.StateType != BankStatementProcessState.Success))
			{
				var errors = Results
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
				}
			}

			return sb.ToString();
		}
	}

	public enum BankStatementFileExtension
	{
		xls,
		xlsx,
		xml
	}
	
	public enum BankStatementBalanceType
	{
		[Display(Name = "исходящий остаток")]
		OutgoingBalance,
		[Display(Name = "исх. сальдо")]
		OutSaldo,
		[Display(Name = "исходящее сальдо")]
		OutgoingSaldo
	}
	
	public enum BankStatementDateType
	{
		[Display(Name = "за")]
		OnDate,
		[Display(Name = "с")]
		FromDate,
		[Display(Name = "конечная дата")]
		EndDate
	}
	
	public enum BankStatementProcessState
	{
		[Display(Name = "Пустая папка")]
		EmptyDirectory,
		[Display(Name = "Успех")]
		Success,
		[Display(Name = "Ошибка парсинга файла")]
		ErrorParsingFile,
		[Display(Name = "Неподдерживаемый формат данных")]
		UnsupportedFileExtension,
		[Display(Name = "Не найден номер расчетного счета")]
		EmptyAccountNumber,
		[Display(Name = "Не найден исходящий остаток")]
		EmptyBalance,
		[Display(Name = "Не найдена дата выписки")]
		EmptyDate,
		[Display(Name = "Неверная дата выписки")]
		WrongBankStatementDate
	}
}
