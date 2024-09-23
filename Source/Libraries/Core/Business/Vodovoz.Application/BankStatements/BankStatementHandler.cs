using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace Vodovoz.Application.BankStatements
{
	/// <summary>
	/// Обработчик распаршенных данных банковской выписки
	/// </summary>
	public class BankStatementHandler
	{
		private const string _failedDirectoryName = "Failed";
		private const string _accountPattern = @"\D*[с|c]ч[е|ё]т\D*";
		private const string _accountNumberPattern = @"[с|c]ч[е|ё]т\D+([0-9]{20,25})";
		private const string _accountNumberWithDatePattern =
			@"[с|c]ч[е|ё]т\D+([0-9]{20,25})\sза\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})\s-\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		//Исходящий остаток на 12.08.2024
		private const string _outgoingBalanceWithDatePattern = @"(Исходящий остаток на \p{Nd}*\.\p{Nd}*\.\p{Nd}*)";
		private const string _balanceWithDatePattern = @"([0-9]{1,}[,|\.][0-9]{1,2})\D+([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		private const string _balancePattern = @"(\d+\s?)+([,|\.]\d+)?$";
		//30.07.2024 Исходящее сальдо дебет: 0 кредит: 6 748,87
		private const string _balanceDebitCreditWithDatePattern = @"([0-9]\s?[0-9]*,*[0-9]*)";
		private const string _credit = "кредит:";
		private const string _singleDatePattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		//с 15.07.2024 по 19.07.2024 | 15.07.2024 - 19.07.2024
		private const string _dateNumberPattern = @"[Сc|Cс]?\s*([0-9]{2}\.[0-9]{2}\.[0-9]{4})\s(по|-)\s([0-9]{2}\.[0-9]{2}\.[0-9]{4})";
		//с 15 июля 2024 по 19 июля 2024
		private const string _dateStringMonthPattern = @"[Сc|Cс]\s([0-9]{2}\D+[0-9]{4})?\D+([0-9]{2}\D+[0-9]{4})";

		private readonly ILogger<BankStatementHandler> _logger;
		private readonly BankStatementParser _parser;
		
		public BankStatementHandler(
			ILogger<BankStatementHandler> logger,
			BankStatementParser parser)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_parser = parser ?? throw new ArgumentNullException(nameof(parser));
		}
		
		public BankStatementProcessedResult ProcessBankStatementsFromDirectory(string directoryPath, DateTime date)
		{
			var bankStatementProcessedResult = new BankStatementProcessedResult();
			var filesPaths = new List<string>();
			var parentDirectoryPath = Directory.GetParent(directoryPath).FullName;

			AddDirectoryFiles(filesPaths, directoryPath);

			if(!filesPaths.Any())
			{
				bankStatementProcessedResult.AddResult(null, BankStatementProcessState.EmptyDirectory);
				return bankStatementProcessedResult;
			}
			
			foreach(var filePath in filesPaths)
			{
				TryParseData(parentDirectoryPath, directoryPath, filePath, date, bankStatementProcessedResult);
			}

			return bankStatementProcessedResult;
		}
		
		private void AddDirectoryFiles(List<string> filesList, string targetDirectory)
		{
			var files = Directory.GetFiles(targetDirectory);
			filesList.AddRange(files);
			
			var subdirectories = Directory.GetDirectories(targetDirectory);
			
			foreach(var subdirectory in subdirectories)
			{
				var directoryName = subdirectory.Split('\\').Last();
				
				if(directoryName == _failedDirectoryName)
				{
					continue;
				}
				
				AddDirectoryFiles(filesList, subdirectory);
			}
		}

		private void TryParseData(
			string parentDirectoryPath,
			string currentDirectoryPath,
			string filePath,
			DateTime date,
			BankStatementProcessedResult bankStatementProcessedResult)
		{
			var successDirectoryPath = parentDirectoryPath + $@"\${date:d}";
			var failedDirectoryPath = currentDirectoryPath + $@"\{_failedDirectoryName}\{date:d}";

			if(!Directory.Exists(successDirectoryPath))
			{
				var successDirectory = Directory.CreateDirectory(successDirectoryPath);
				successDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			}
			
			if(!Directory.Exists(failedDirectoryPath))
			{
				Directory.CreateDirectory(failedDirectoryPath);
			}
			
			var fileExtension = CheckFileExtension(filePath);
			if(fileExtension is null)
			{
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.UnsupportedFileExtension);
				TryMoveFileToDirectory(failedDirectoryPath, filePath, false);
				return;
			}
			
			TryParseData(successDirectoryPath, failedDirectoryPath, filePath, fileExtension.Value, date, bankStatementProcessedResult);
		}

		private void TryParseData(
			string successDirectoryPath,
			string failedDirectoryPath,
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
					TryMoveFileToDirectory(failedDirectoryPath, filePath, false);
					return;
				}

				bankStatementProcessedResult.AddSuccessResult(filePath, result.AccountNumber, result.Balance.Value, result.Date.Value);
				TryMoveFileToDirectory(successDirectoryPath, filePath, true);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при парсинге файла выписки {FilePath}", filePath);
				bankStatementProcessedResult.AddResult(filePath, BankStatementProcessState.ErrorParsingFile);
				TryMoveFileToDirectory(failedDirectoryPath, filePath, false);
			}
		}

		private void TryMoveFileToDirectory(string directoryPath, string filePath, bool success)
		{
			var directoryMessage = success
				? " в папку успешной обработки на день"
				: " в папку провальных файлов";

			var fileName = Path.GetFileName(filePath);
			var newFilePath = directoryPath + $@"\{fileName}";
			
			try
			{
				if(File.Exists(newFilePath))
				{
					var guid = Guid.NewGuid();
					File.Move(filePath, directoryPath + $@"\{guid}_{fileName}");
				}
				else
				{
					File.Move(filePath, newFilePath);
				}
			}
			catch(Exception exc)
			{
				_logger.LogError(exc, "Ошибка при перемещения файла {FilePath}" + directoryMessage, filePath);
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
							date = dateNumberMatches[dateNumberMatches.Count - 1].Groups[3].Value;
						}
						
						if(dateStringMonthMatches.Count != 0)
						{
							date = dateStringMonthMatches[dateStringMonthMatches.Count - 1].Groups[2].Value;
						}

						if(TryParseDate(date) is null)
						{
							date = null;
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
				
				var cellWithoutSpaces = string.Empty;
				
				foreach(var cell in row)
				{
					var accountMatches = Regex.Matches(cell.ToLower(), _accountPattern);

					if(accountMatches.Count != 0)
					{
						success = true;
					}

					//в некоторых выписках номер расчетного счета с пробелами
					cellWithoutSpaces = RemoveSpacesFromString(cell);
					
					if(success)
					{
						result += $" {cellWithoutSpaces}";
					}
				}

				if(success)
				{
					var accountNumberMatches = Regex.Matches(result.ToLower().Trim(' '), _accountNumberPattern);

					if(accountNumberMatches.Count != 0)
					{
						accountNumber = accountNumberMatches[accountNumberMatches.Count - 1].Groups[1].Value;
					}
					else
					{
						result = string.Empty;
						success = false;
					}
				}
			}
		}

		private string RemoveSpacesFromString(string cell)
		{
			return cell.Where(ch => ch != ' ')
				.Aggregate(string.Empty, (current, ch) => current + ch);
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
						TryProcessPeriodNumberDate(dateRow.Data, ref date);
						break;
					case BankStatementDateType.OnDate:
					case BankStatementDateType.StatementOnDate:
						TryProcessSingleDate(dateRow.Data, ref date);
						break;
					case BankStatementDateType.FromPeriodDate:
						TryProcessPeriodStringDate(dateRow.Data, ref date);
						break;
					case BankStatementDateType.EndDate:
						TryProcessSingleDate(dateRow.Data, ref date);
						break;
				}
			}
		}

		private void TryProcessPeriodNumberDate(string dateRow, ref string date)
		{
			var dateMatches = Regex.Matches(dateRow, _dateNumberPattern);

			if(dateMatches.Count != 0)
			{
				date = dateMatches[dateMatches.Count - 1].Groups[3].Value;
			}
		}
		
		private void TryProcessPeriodStringDate(string dateRow, ref string date)
		{
			var dateMatches = Regex.Matches(dateRow, _dateStringMonthPattern);

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
						case BankStatementDateType.StatementOnDate:
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

					if(string.IsNullOrWhiteSpace(balance))
					{
						(balanceRow.Data as List<string>).Add(parsedData.Last().LastOrDefault());
						TryProcessOutgoingBalance(balanceRow.Data, ref balance);
					}
					break;
				case BankStatementBalanceType.BalanceOutgoing:
					TryProcessBalanceOutgoing(balanceRow.Data, ref balance);
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
			//для выписок где в одной строке указан и дебет и кредит
			var balanceValues = new List<string>();
			
			foreach(var cell in data)
			{
				var balanceOnDateMatches = Regex.Matches(cell, _balanceWithDatePattern);
				var outgoingBalanceWithDatePattern = Regex.Matches(cell, _outgoingBalanceWithDatePattern);
				var balanceMatches = Regex.Matches(cell, _balancePattern);
				
				if(outgoingBalanceWithDatePattern.Count != 0)
				{
					//чтобы не подцепить дату за баланс, пропускаем
					continue;
				}

				if(balanceOnDateMatches.Count != 0)
				{
					balance = balanceOnDateMatches[balanceOnDateMatches.Count - 1].Groups[1].Value;
					break;
				}

				if(balanceMatches.Count != 0)
				{
					if(balanceValues.Count > 1)
					{
						break;
					}
					
					balanceValues.Add(balanceMatches[balanceMatches.Count - 1].Value);
				}
			}

			if(!string.IsNullOrWhiteSpace(balance))
			{
				return;
			}

			if(balanceValues.Any())
			{
				if(balanceValues.Count == 1)
				{
					balance = balanceValues[0];
				}
				else if(balanceValues.Count == 2)
				{
					var debitBalance = TryParseBalance(balanceValues[0]);
					var creditBalance = TryParseBalance(balanceValues[1]);

					if(debitBalance.HasValue && creditBalance.HasValue)
					{
						balance = (creditBalance.Value - debitBalance.Value).ToString();
					}
				}
			}
		}
		
		private void TryProcessBalanceOutgoing(IEnumerable<string> data, ref string balance)
		{
			foreach(var cell in data)
			{
				var balanceMatches = Regex.Matches(cell, _balancePattern);

				if(balanceMatches.Count != 0)
				{
					balance = balanceMatches[balanceMatches.Count - 1].Value;
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
						balance = balanceMatches[balanceMatches.Count - 1].Value;
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
					var str = cell.Split(' ', '\u00a0');

					if(str.Contains(_credit))
					{
						var sb = new StringBuilder();
						var isCreditValue = false;
						
						foreach(var s in str)
						{
							if(isCreditValue)
							{
								sb.Append(s);
								continue;
							}
							
							if(s == _credit)
							{
								isCreditValue = true;
							}
						}

						if(sb.Length > 0)
						{
							balance = sb.ToString();
						}
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
			
			balance = balance.Replace('.', ',');

			if(decimal.TryParse(balance, NumberStyles.Any, CultureInfo.CurrentUICulture, out var convertedBalance1))
			{
				return convertedBalance1;
			}

			balance = balance.Replace(',', '.');

			if(decimal.TryParse(balance, NumberStyles.Any, CultureInfo.InvariantCulture, out var convertedBalance2))
			{
				return convertedBalance2;
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
}
