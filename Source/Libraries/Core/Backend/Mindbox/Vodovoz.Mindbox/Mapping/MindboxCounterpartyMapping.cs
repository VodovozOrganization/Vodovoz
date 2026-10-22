using System;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Mindbox.Mapping
{
	/// <summary>
	/// Правила преобразования контрагента Доставки воды в поля Mindbox
	/// </summary>
	public static class MindboxCounterpartyMapping
	{
		public const string PhysicalCounterpartyForm = "physical";
		public const string LegalCounterpartyForm = "legal";

		public static string GetCounterpartyForm(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			return counterparty.PersonType == PersonType.legal
				? LegalCounterpartyForm
				: PhysicalCounterpartyForm;
		}

		public static string GetCounterpartyType(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			switch(counterparty.CounterpartyType)
			{
				case CounterpartyType.Supplier:
					return "supplier";
				case CounterpartyType.Dealer:
					return "dealer";
				case CounterpartyType.AdvertisingDepartmentClient:
					return "advertisingDepartmentClient";
				default:
					return "customer";
			}
		}

		public static string GetPaymentForm(PaymentType paymentType)
		{
			switch(paymentType)
			{
				case PaymentType.Cashless:
					return "cashless";
				case PaymentType.Terminal:
					return "terminal";
				case PaymentType.DriverApplicationQR:
					return "driverApplicationQr";
				case PaymentType.SmsQR:
					return "smsQr";
				case PaymentType.PaidOnline:
					return "paidOnline";
				case PaymentType.Barter:
					return "barter";
				case PaymentType.ContractDocumentation:
					return "contractDocumentation";
				default:
					return "cash";
			}
		}

		public static string GetSourcePointOfContact(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			return counterparty.CameFrom?.Name;
		}

		public static string GetMobilePhone(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			var phone = counterparty.Phones?
				.Where(p => !p.IsArchive && !string.IsNullOrWhiteSpace(p.DigitsNumber))
				.OrderBy(p => p.Id)
				.FirstOrDefault()
				?? counterparty.Phones?
					.Where(p => !string.IsNullOrWhiteSpace(p.DigitsNumber))
					.OrderBy(p => p.Id)
					.FirstOrDefault();

			if(phone == null)
			{
				return null;
			}

			var digits = new string(phone.DigitsNumber.Where(char.IsDigit).ToArray());

			if(digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
			{
				digits = digits.Substring(1);
			}

			return digits.Length == 10 ? $"+7{digits}" : null;
		}

		public static string GetEmail(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			return counterparty.Emails?
				.Where(e => !string.IsNullOrWhiteSpace(e.Address))
				.OrderBy(e => e.Id)
				.Select(e => e.Address)
				.FirstOrDefault();
		}

		public static DeliveryPoint GetDeliveryPoint(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			return counterparty.DeliveryPoints?
				.OrderByDescending(dp => dp.IsActive)
				.ThenBy(dp => dp.Id)
				.FirstOrDefault();
		}
	}
}
