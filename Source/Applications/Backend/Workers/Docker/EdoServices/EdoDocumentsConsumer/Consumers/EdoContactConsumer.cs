using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdoDocumentsConsumer.Converters;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace EdoDocumentsConsumer.Consumers
{
	public class EdoContactConsumer : IConsumer<EdoContactInfo>
	{
		private readonly ILogger<EdoContactConsumer> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IEdoContactStateCodeConverter _contactStateConverter;

		public EdoContactConsumer(
			ILogger<EdoContactConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICounterpartyRepository counterpartyRepository,
			IEdoContactStateCodeConverter contactStateConverter
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_contactStateConverter = contactStateConverter ?? throw new ArgumentNullException(nameof(contactStateConverter));
		}
		
		public Task Consume(ConsumeContext<EdoContactInfo> context)
		{
			var contact = context.Message;
			
			_logger.LogInformation(
				"Обрабатываем данные поступившего контакта. ИНН {Inn}",
				contact.Inn);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Обновление данных контакта с ИНН {contact.Inn}"))
			{
				IList<Counterparty> counterparties;
				if(!Enum.TryParse<EdoContactStateCode>(contact.StateCode, out var stateCode))
				{
					//TODO что будем делать, если не распарсилось состояние
					stateCode = EdoContactStateCode.Error;
				}

				switch(stateCode)
				{
					//TODO подумать как будем обрабатывать входящие приглашения
					/*case EdoContactStateCode.Incoming:
						_logger.LogInformation(
							"Входящее приглашение от клиента с аккаунтом {EdxClientId}...", contact.EdxClientId);
						_taxcomApi.AcceptContact(contact.EdxClientId);

						counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

						if(counterparties == null)
						{
							break;
						}

						foreach(var counterparty in counterparties)
						{
							_logger.LogInformation("Обновляем данные у клиента Id {CounterpartyId}", counterparty.Id);
							counterparty.EdoOperator =
								_counterpartyRepository.GetEdoOperatorByCode(uow, contact.EdxClientId[..3]);
							counterparty.PersonalAccountIdInEdo = contact.EdxClientId;
							counterparty.ConsentForEdoStatus = ConsentForEdoStatus.Agree;

							uow.Save(counterparty);
							uow.Commit();
						}
						break;*/
					case EdoContactStateCode.Sent:
					case EdoContactStateCode.Accepted:
					case EdoContactStateCode.Rejected:
					case EdoContactStateCode.Error:
						_logger.LogInformation("Обрабатываем контакт в статусе {StateCode}", contact.StateCode);
						counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

						if(counterparties == null)
						{
							break;
						}

						var consentForEdoStatus =
							_contactStateConverter.ConvertStateToConsentForEdoStatus(stateCode);
						
						foreach(var counterparty in counterparties)
						{
							if(counterparty.ConsentForEdoStatus == consentForEdoStatus)
							{
								continue;
							}

							_logger.LogInformation(
								"Обновляем согласие на ЭДО у клиента Id {CounterpartyId}" +
								" с {CounterpartyConsentForEdoStatus} на {ConsentForEdoStatus}",
								counterparty.Id, counterparty.ConsentForEdoStatus, consentForEdoStatus);
							
							if(consentForEdoStatus == ConsentForEdoStatus.Rejected)
							{
								if(counterparty.PersonalAccountIdInEdo != contact.EdxClientId)
								{
									_logger.LogInformation(
										"Пришел отказ на ЭДО у клиента Id {CounterpartyId}" +
										" по кабинету {EdxClientId}," +
										" хотя у клиента {CounterpartyPersonalAccountIdInEdo} пропускаем...",
										counterparty.Id, contact.EdxClientId, counterparty.PersonalAccountIdInEdo);
									continue;
								}
							}
							else if(consentForEdoStatus == ConsentForEdoStatus.Agree)
							{
								counterparty.PersonalAccountIdInEdo = contact.EdxClientId;
								counterparty.EdoOperator =
									_counterpartyRepository.GetEdoOperatorByCode(uow, contact.EdxClientId[..3]);
							}
							
							counterparty.ConsentForEdoStatus = consentForEdoStatus;
							uow.Save(counterparty);
							uow.Commit();
						}

						break;
				}
			}
			
			return Task.CompletedTask;
		}
	}
}
