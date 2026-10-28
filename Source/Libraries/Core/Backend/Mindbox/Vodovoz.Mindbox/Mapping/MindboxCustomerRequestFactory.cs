using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Mindbox.Contracts;

namespace Vodovoz.Mindbox.Mapping
{
	/// <summary>
	/// Формирует тело запроса на создание/обновление клиента в Mindbox
	/// </summary>
	public interface IMindboxCustomerRequestFactory
	{
		MindboxCustomerRequest Create(IUnitOfWork unitOfWork, Counterparty counterparty);
	}

	/// <inheritdoc />
	public sealed class MindboxCustomerRequestFactory : IMindboxCustomerRequestFactory
	{
		private readonly ILogger<MindboxCustomerRequestFactory> _logger;

		public MindboxCustomerRequestFactory(ILogger<MindboxCustomerRequestFactory> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public MindboxCustomerRequest Create(IUnitOfWork unitOfWork, Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			var counterpartyForm = MindboxCounterpartyMapping.GetCounterpartyForm(counterparty);

			var customer = new MindboxCustomer
			{
				Ids = new MindboxCustomerIds
				{
					ErpId = counterparty.Id.ToString(CultureInfo.InvariantCulture)
				},
				MobilePhone = MindboxCounterpartyMapping.GetMobilePhone(unitOfWork, counterparty),
				Email = MindboxCounterpartyMapping.GetEmail(counterparty),
				CustomFields = new MindboxCustomerCustomFields
				{
					CounterpartyForm = counterpartyForm
				}
			};

			if(counterparty.PersonType == PersonType.natural)
			{
				customer.LastName = counterparty.Surname;
				customer.FirstName = counterparty.FirstName;
				customer.MiddleName = counterparty.Patronymic;
				customer.FullName = null;
			}
			else
			{
				customer.LastName = null;
				customer.FirstName = null;
				customer.MiddleName = null;
				customer.FullName = MindboxCounterpartyMapping.GetFullName(counterparty);
			}

			LogSkippedContacts(counterparty, customer);

			return new MindboxCustomerRequest
			{
				Customer = customer
			};
		}

		private void LogSkippedContacts(Counterparty counterparty, MindboxCustomer customer)
		{
			if(customer.MobilePhone == null)
			{
				var phone = counterparty.Phones?
					.Where(p => !string.IsNullOrWhiteSpace(p.DigitsNumber))
					.Select(p => p.DigitsNumber)
					.FirstOrDefault();

				if(phone != null)
				{
					_logger.LogWarning(
						"Телефон контрагента {ErpId} не прошел проверку формата и не будет отправлен в Mindbox: {Phone}",
						counterparty.Id,
						MindboxContactValidator.MaskPhone(phone));
				}
			}

			if(customer.Email == null)
			{
				var email = counterparty.Emails?
					.Where(e => !string.IsNullOrWhiteSpace(e.Address))
					.Select(e => e.Address)
					.FirstOrDefault();

				if(email != null)
				{
					_logger.LogWarning(
						"E-mail контрагента {ErpId} не прошел проверку формата и не будет отправлен в Mindbox: {Email}",
						counterparty.Id,
						MindboxContactValidator.MaskEmail(email));
				}
			}
		}
	}
}
