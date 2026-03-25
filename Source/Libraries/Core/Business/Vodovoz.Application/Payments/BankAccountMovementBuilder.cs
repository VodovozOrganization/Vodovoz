using System;
using System.Collections.Generic;
using System.Globalization;
using QS.Banks.Domain;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Application.Payments
{
	/// <summary>
	/// Билдер для создания сущности движения средств по р/сч <see cref="BankAccountMovement"/>
	/// </summary>
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

		/// <summary>
		/// Добавление начальной даты
		/// </summary>
		/// <param name="date">дата</param>
		/// <returns></returns>
		public BankAccountMovementBuilder StartDate(DateTime date)
		{
			_bankAccountMovement.StartDate = date;
			return this;
		}
		
		/// <summary>
		/// Добавление конечной даты
		/// </summary>
		/// <param name="date">дата</param>
		/// <returns></returns>
		public BankAccountMovementBuilder EndDate(DateTime date)
		{
			_bankAccountMovement.EndDate = date;
			return this;
		}

		/// <summary>
		/// Добавление информации о банке
		/// </summary>
		/// <param name="bank">Банк</param>
		/// <returns></returns>
		public BankAccountMovementBuilder Bank(Bank bank)
		{
			_bankAccountMovement.Bank = bank;
			return this;
		}
		
		/// <summary>
		/// Добавление информации о расчетном счете
		/// </summary>
		/// <param name="account">Р/сч</param>
		/// <returns></returns>
		public BankAccountMovementBuilder Account(Account account)
		{
			_bankAccountMovement.Account = account;
			return this;
		}
		
		/// <summary>
		/// Добавление номера р/сч
		/// </summary>
		/// <param name="accountNumber">Номер р/сч</param>
		/// <returns></returns>
		public BankAccountMovementBuilder AccountNumber(string accountNumber)
		{
			_bankAccountMovement.AccountNumber = accountNumber;
			return this;
		}
		
		/// <summary>
		/// Добавление входящего остатка
		/// </summary>
		/// <param name="initialBalance">Входящий остатка</param>
		/// <returns></returns>
		public BankAccountMovementBuilder InitialBalance(decimal initialBalance)
		{
			_bankAccountMovement.AddData(initialBalance, BankAccountMovementDataType.InitialBalance);
			return this;
		}
		
		/// <summary>
		/// Добавление исходящего остатка
		/// </summary>
		/// <param name="finalBalance">Исходящий остатка</param>
		/// <returns></returns>
		public BankAccountMovementBuilder FinalBalance(decimal finalBalance)
		{
			_bankAccountMovement.AddData(finalBalance, BankAccountMovementDataType.FinalBalance);
			return this;
		}
		
		/// <summary>
		/// Добавление крЕдита(всего поступило)
		/// </summary>
		/// <param name="totalReceived">Кредит</param>
		/// <returns></returns>
		public BankAccountMovementBuilder TotalReceived(decimal totalReceived)
		{
			_bankAccountMovement.AddData(totalReceived, BankAccountMovementDataType.TotalReceived);
			return this;
		}
		
		/// <summary>
		/// Добавление дебета
		/// </summary>
		/// <param name="totalWrittenOff">Дебет</param>
		/// <returns></returns>
		public BankAccountMovementBuilder TotalWrittenOff(decimal totalWrittenOff)
		{
			_bankAccountMovement.AddData(totalWrittenOff, BankAccountMovementDataType.TotalWrittenOff);
			return this;
		}

		/// <summary>
		/// Добавление различной информации
		/// </summary>
		/// <param name="accountData">Добавляемая информация в виде ключ-значение</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
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

		/// <summary>
		/// Создание сущности <see cref="BankAccountMovement"/>
		/// </summary>
		/// <returns></returns>
		public BankAccountMovement Build()
		{
			var accountMovement = _bankAccountMovement;
			_bankAccountMovement = new BankAccountMovement();
			
			return accountMovement;
		}

		public static BankAccountMovementBuilder Create() => new BankAccountMovementBuilder();
	}
}
