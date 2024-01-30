using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EdoService.Library.Converters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Taxcom.Client.Api;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;

namespace TaxcomEdoApi.Services
{
	public class ContactsUpdaterService : BackgroundService
	{
		private readonly ILogger<ContactsUpdaterService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IParametersProvider _parametersProvider;
		private readonly IContactStateConverter _contactStateConverter;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private const int _delaySec = 120;

		public ContactsUpdaterService(
			ILogger<ContactsUpdaterService> logger,
			TaxcomApi taxcomApi,
			IUnitOfWorkFactory unitOfWorkFactory,
			IParametersProvider parametersProvider,
			IContactStateConverter contactStateConverter,
			ICounterpartyRepository counterpartyRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_contactStateConverter = contactStateConverter ?? throw new ArgumentNullException(nameof(contactStateConverter));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс обновления контактов запущен");
			var lastCheckContactsUpdates = _parametersProvider.GetValue<DateTime>("last_check_contacts_updates");
			await StartWorkingAsync(stoppingToken, lastCheckContactsUpdates);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken, DateTime lastCheckContactsUpdates)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);

				try
				{
					_logger.LogInformation("Обновление списка контактов...");

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
					{
						var contactUpdates = new ContactList();

						do
						{
							try
							{
								_logger.LogInformation("Получаем список...");
								var response = _taxcomApi.GetContactListUpdates(lastCheckContactsUpdates, null);
								contactUpdates = ContactListSerializer.DeserializeContactList(response);
							}
							catch(Exception e)
							{
								_logger.LogError(e, "Ошибка при запросе списка контактов");
							}

							if(contactUpdates.Contacts is null)
							{
								break;
							}

							foreach(var contact in contactUpdates.Contacts)
							{
								IList<Counterparty> counterparties;

								switch(contact.State.Code)
								{
									case ContactStateCode.Incoming:
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
										break;
									case ContactStateCode.Sent:
									case ContactStateCode.Accepted:
									case ContactStateCode.Rejected:
									case ContactStateCode.Error:
										_logger.LogInformation("Обрабатываем контакт в статусе {StateCode}", contact.State.Code);
										counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

										if(counterparties == null)
										{
											break;
										}

										var consentForEdoStatus =
											_contactStateConverter.ConvertStateToConsentForEdoStatus(contact.State.Code);
										
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

								lastCheckContactsUpdates = contact.State.Changed;
							}
						} while(contactUpdates.Contacts != null && contactUpdates.Contacts.Length >= 100);
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при обновлении списка контактов");
				}
				finally
				{
					_parametersProvider.CreateOrUpdateParameter("last_check_contacts_updates", $"{lastCheckContactsUpdates:s}");
				}
			}
		}

		private async Task DelayAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Ждем {DelaySec}сек", _delaySec);
			await Task.Delay(_delaySec * 1000, stoppingToken);
		}
	}
}
