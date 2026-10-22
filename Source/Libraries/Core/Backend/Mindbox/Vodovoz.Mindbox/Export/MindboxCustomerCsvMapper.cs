using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Mindbox.Mapping;

namespace Vodovoz.Mindbox.Export
{
	/// <summary>
	/// Формирует строки CSV для операции массового импорта клиентов Mindbox
	/// </summary>
	public interface IMindboxCustomerCsvMapper
	{
		/// <summary>
		/// Заголовок CSV
		/// </summary>
		string Header { get; }

		/// <summary>
		/// Формирует строку CSV по контрагенту
		/// </summary>
		string Map(Counterparty counterparty);
	}

	/// <inheritdoc />
	public sealed class MindboxCustomerCsvMapper : IMindboxCustomerCsvMapper
	{
		private const string ColumnDelimiter = ";";

		/// <inheritdoc />
		public string Header =>
			"Websiteid"
			+ ColumnDelimiter + "FirstName"
			+ ColumnDelimiter + "LastName"
			+ ColumnDelimiter + "MiddleName"
			+ ColumnDelimiter + "BirthDate"
			+ ColumnDelimiter + "MobilePhone"
			+ ColumnDelimiter + "Email"
			+ ColumnDelimiter + "SourceDateTimeUtc"
			+ ColumnDelimiter + "SourcePointOfContact"
			+ ColumnDelimiter + "IsSubscribed"
			+ ColumnDelimiter + "ERPcounterpartyId"
			+ ColumnDelimiter + "CounterpartyForm"
			+ ColumnDelimiter + "CounterpartyType"
			+ ColumnDelimiter + "IsNet"
			+ ColumnDelimiter + "OwnershipForm"
			+ ColumnDelimiter + "CounterpartyName"
			+ ColumnDelimiter + "CounterpartyFullName"
			+ ColumnDelimiter + "PaymentForm"
			+ ColumnDelimiter + "INN"
			+ ColumnDelimiter + "KPP";

		/// <inheritdoc />
		public string Map(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			var email = MindboxCounterpartyMapping.GetEmail(counterparty);

			var values = new List<string>
			{
				counterparty.Id.ToString(CultureInfo.InvariantCulture),
				counterparty.FirstName,
				counterparty.Surname,
				counterparty.Patronymic,
				string.Empty,
				MindboxCounterpartyMapping.GetMobilePhone(counterparty),
				email,
				FormatDate(counterparty.CreateDate),
				MindboxCounterpartyMapping.GetSourcePointOfContact(counterparty),
				string.IsNullOrWhiteSpace(email) ? "false" : "true",
				counterparty.Id.ToString(CultureInfo.InvariantCulture),
				MindboxCounterpartyMapping.GetCounterpartyForm(counterparty),
				MindboxCounterpartyMapping.GetCounterpartyType(counterparty),
				counterparty.IsChainStore ? "true" : "false",
				counterparty.TypeOfOwnership,
				counterparty.Name,
				string.IsNullOrWhiteSpace(counterparty.FullName) ? counterparty.Name : counterparty.FullName,
				MindboxCounterpartyMapping.GetPaymentForm(counterparty.PaymentMethod),
				counterparty.INN,
				counterparty.KPP
			};

			return string.Join(ColumnDelimiter, values.Select(Escape));
		}

		private static string FormatDate(DateTime? date)
		{
			if(!date.HasValue)
			{
				return string.Empty;
			}

			return date.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		private static string Escape(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			var requiresQuotes = value.IndexOf(ColumnDelimiter, StringComparison.Ordinal) >= 0
				|| value.IndexOf('"') >= 0
				|| value.IndexOf('\r') >= 0
				|| value.IndexOf('\n') >= 0;

			if(!requiresQuotes)
			{
				return value;
			}

			return $"\"{value.Replace("\"", "\"\"")}\"";
		}
	}
}
