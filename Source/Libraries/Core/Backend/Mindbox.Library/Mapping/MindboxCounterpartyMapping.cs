using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Правила преобразования контрагента Доставки воды в поля Mindbox
	/// </summary>
	public static class MindboxCounterpartyMapping
	{
		public const string PhysicalCounterpartyForm = "Physical";
		public const string LegalCounterpartyForm = "Legal";

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

		/// <summary>
		/// Для физического лица приоритет у телефона, который является логином в мобильном приложении.
		/// </summary>
		public static string GetMobilePhone(IUnitOfWork unitOfWork, Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			if(counterparty.PersonType == PersonType.natural && unitOfWork != null)
			{
				var mobileAppPhone = GetMobileAppPhone(unitOfWork, counterparty.Id);
				var normalizedMobileAppPhone = MindboxContactValidator.NormalizePhone(mobileAppPhone);

				if(normalizedMobileAppPhone != null)
				{
					return normalizedMobileAppPhone;
				}
			}

			var phone = counterparty.Phones?
				.Where(p => !p.IsArchive && !string.IsNullOrWhiteSpace(p.DigitsNumber))
				.OrderBy(p => p.Id)
				.FirstOrDefault()
				?? counterparty.Phones?
					.Where(p => !string.IsNullOrWhiteSpace(p.DigitsNumber))
					.OrderBy(p => p.Id)
					.FirstOrDefault();

			return MindboxContactValidator.NormalizePhone(phone?.DigitsNumber);
		}

		/// <summary>
		/// Возвращает первый корректный e-mail контрагента
		/// </summary>
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
				.FirstOrDefault(MindboxContactValidator.IsValidEmail);
		}

		/// <summary>
		/// Для юридического лица — полное название, для физического fullName не заполняется.
		/// </summary>
		public static string GetFullName(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			return string.IsNullOrWhiteSpace(counterparty.FullName)
				? counterparty.Name
				: counterparty.FullName;
		}

		private static string GetMobileAppPhone(IUnitOfWork unitOfWork, int counterpartyId)
		{
			var query = from phone in unitOfWork.Session.Query<Phone>()
				join externalCounterparty in unitOfWork.Session.Query<ExternalCounterparty>()
					on phone.Id equals externalCounterparty.Phone.Id
				where phone.Counterparty.Id == counterpartyId
					&& !phone.IsArchive
					&& externalCounterparty.CounterpartyFrom == CounterpartyFrom.MobileApp
					&& !externalCounterparty.IsArchive
				orderby externalCounterparty.Id descending
				select phone.DigitsNumber;

			return query.FirstOrDefault();
		}
	}
}
