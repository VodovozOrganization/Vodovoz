using System;
using System.Collections.Generic;
using System.Globalization;
using QS.Banks.Domain;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Application.Payments
{
	public class BankAccountMovementBuilder
	{
		private BankAccountMovement _bankAccountMovement;
		private CultureInfo _culture;
		
		public BankAccountMovementBuilder()
		{
			_bankAccountMovement = new BankAccountMovement();
			_culture = CultureInfo.CreateSpecificCulture("ru-RU");
			_culture.NumberFormat.NumberDecimalSeparator = ".";
		}

		public BankAccountMovementBuilder StartDate(DateTime date)
		{
			_bankAccountMovement.StartDate = date;
			return this;
		}
		
		public BankAccountMovementBuilder EndDate(DateTime date)
		{
			_bankAccountMovement.EndDate = date;
			return this;
		}

		public BankAccountMovementBuilder Bank(Bank bank)
		{
			_bankAccountMovement.Bank = bank;
			return this;
		}
		
		public BankAccountMovementBuilder Account(Account account)
		{
			_bankAccountMovement.Account = account;
			return this;
		}
		
		public BankAccountMovementBuilder AccountNumber(string accountNumber)
		{
			_bankAccountMovement.AccountNumber = accountNumber;
			return this;
		}
		
		public BankAccountMovementBuilder InitialBalance(decimal initialBalance)
		{
			_bankAccountMovement.AddData(initialBalance, BankAccountMovementDataType.InitialBalance);
			return this;
		}
		
		public BankAccountMovementBuilder FinalBalance(decimal finalBalance)
		{
			_bankAccountMovement.AddData(finalBalance, BankAccountMovementDataType.FinalBalance);
			return this;
		}
		
		public BankAccountMovementBuilder TotalReceived(decimal totalReceived)
		{
			_bankAccountMovement.AddData(totalReceived, BankAccountMovementDataType.TotalReceived);
			return this;
		}
		
		public BankAccountMovementBuilder TotalWrittenOff(decimal totalWrittenOff)
		{
			_bankAccountMovement.AddData(totalWrittenOff, BankAccountMovementDataType.TotalWrittenOff);
			return this;
		}

		public BankAccountMovementBuilder AddData(KeyValuePair<string, string> accountData)
		{
			switch(accountData.Key)
			{
				case "ДатаНачала":
					StartDate(DateTime.Parse(accountData.Value));
					break;
				case "ДатаКонца":
					EndDate(DateTime.Parse(accountData.Value));
					break;
				case "РасчСчет":
					AccountNumber(accountData.Value);
					break;
				case "НачальныйОстаток":
					InitialBalance(decimal.Parse(accountData.Value, _culture));
					break;
				case "ВсегоПоступило":
					TotalReceived(decimal.Parse(accountData.Value, _culture));
					break;
				case "ВсегоСписано":
					TotalWrittenOff(decimal.Parse(accountData.Value, _culture));
					break;
				case "КонечныйОстаток":
					FinalBalance(decimal.Parse(accountData.Value, _culture));
					break;
				default:
					throw new ArgumentException($"{accountData.Key} неизвестный параметр секции расчетных счетов");
			}

			return this;
		}

		public BankAccountMovement Build()
		{
			var accountMovement = _bankAccountMovement;
			_bankAccountMovement = new BankAccountMovement();
			
			return accountMovement;
		}

		public static BankAccountMovementBuilder Create() => new BankAccountMovementBuilder();
	}
}
