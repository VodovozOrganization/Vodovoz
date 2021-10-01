using System;
using System.Collections.Generic;
using System.Linq;
using Bitrix;
using Bitrix.DTO;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozInfrastructure.Utils;
using Contact = Bitrix.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;
using BitrixPhone = Bitrix.DTO.Phone;

namespace BitrixIntegration.Processors
{
	public class CounterpartyProcessor : ICounterpartyProcessor
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IBitrixClient _bitrixClient;
		private readonly ICounterpartyRepository _counterpartyRepository;
		
		public CounterpartyProcessor(IBitrixClient bitrixClient, ICounterpartyRepository counterpartyRepository)
		{
			_bitrixClient = bitrixClient;
			_counterpartyRepository = counterpartyRepository;
		}
		
		public Counterparty ProcessCounterparty(IUnitOfWork uow, Deal deal)
		{
			if(deal.ContactId != 0)
			{
				return GetContact(uow, deal);
			}

			if(deal.CompanyId != 0)
			{
				return GetCompany(uow, deal);
			}

			throw new InvalidOperationException("Сделка не имеет ни контакта, ни компании, такие сделки невозможно обработать");
		}

		private Counterparty GetContact(IUnitOfWork uow, Deal deal)
		{
			_logger.Info("Обработка контрагента как контакта");

			var contact = _bitrixClient.GetContact(deal.ContactId).GetAwaiter().GetResult()
			              ?? throw new InvalidOperationException($"Не удалось загрузить контакт №{deal.ContactId}");

			Counterparty counterparty = GetCounterpartyOrNull(uow, contact);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для контакта: {contact.Id} {contact.SecondName} {contact.Name} {contact.LastName} " +
				             ", создаем нового контрагента");
				counterparty = new Counterparty()
				{
					FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					Name = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					BitrixId = contact.Id,
					PersonType = PersonType.natural,
					CreateDate = contact.CreatedDate,
					PaymentMethod = deal.GetPaymentMethod(),
					IsArchive = false
				};
				AddPhonesToCounterparty(counterparty, contact.Phones);
			}

			return counterparty;
		}

		private Counterparty GetCompany(IUnitOfWork uow, Deal deal)
		{
			_logger.Info("Обработка контрагента как компании");

			var company = _bitrixClient.GetCompany(deal.CompanyId).GetAwaiter().GetResult()
			              ?? throw new InvalidOperationException($"Не удалось загрузить компанию №{deal.CompanyId}");

			Counterparty counterparty = GetCounterpartyOrNull(uow, company);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для компании: {company.Title}, создаем нового контрагента");
				counterparty = new Counterparty()
				{
					FullName = company.Title,
					Name = company.Title,
					BitrixId = company.Id,
					PersonType = PersonType.legal,
					TypeOfOwnership = NamesUtils.TryGetOrganizationType(company.Title) ?? "",
					CreateDate = company.DateCreate,
					PaymentMethod = deal.GetPaymentMethod(),
					IsArchive = false
				};
				AddPhonesToCounterparty(counterparty, company.Phones);
			}

			return counterparty;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Contact contact)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, contact.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = contact.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			string contactName;
			if(!string.IsNullOrWhiteSpace(contact.LastName))
			{
				contactName = contact.LastName;
			}
			else if(!string.IsNullOrWhiteSpace(contact.Name))
			{
				contactName = contact.Name;
			}
			else if(!string.IsNullOrWhiteSpace(contact.SecondName))
			{
				contactName = contact.SecondName;
			}
			else
			{
				throw new InvalidOperationException("Контакт не содержит имени, фамилии и отчества, необходимых для поиска контрагента");
			}

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, contactName, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для контакта с BitrixId {contact.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и части имени");
				return counterparty;
			}

			if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для контакта с BitrixId {contact.Id} найдено несколько контрагентов ({counterpartyIds}) " +
				             $"по телефону и части имени. Невозможно выбрать кого-то одного");
				return null;
			}

			_logger.Info($"Для контакта с BitrixId {contact.Id} не найдено контрагентов по телефону и части имени");
			return null;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Company company)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, company.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = company.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			if(string.IsNullOrWhiteSpace(company.Title))
			{
				throw new InvalidOperationException("Компания не содержит названия, необходимого для поиска контрагента");
			}

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, company.Title, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для компании с BitrixId {company.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и названию");
				return counterparty;
			}

			if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для компании с BitrixId {company.Id} найдено несколько контрагентов ({counterpartyIds}) " +
				             $"по телефону и названию. Невозможно выбрать кого-то одного");
				return null;
			}

			_logger.Info($"Для компании с BitrixId {company.Id} не найдено контрагентов по телефону и названию");
			return null;
		}

		private void AddPhonesToCounterparty(Counterparty counterparty, IEnumerable<BitrixPhone> phones)
		{
			foreach(var contactPhone in phones)
			{
				var phone = new Phone
				{
					Number = contactPhone.Value
				};
				_logger.Info($"Добавляем телефон: {contactPhone} контрагенту {counterparty.FullName}");
				counterparty.Phones.Add(phone);
			}
		}
	}
}
