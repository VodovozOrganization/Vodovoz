using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EdoContactsUpdater.Configs;
using EdoContactsUpdater.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Services;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Settings;
using Vodovoz.Zabbix.Sender;

namespace EdoContactsUpdater
{
	public class TaxcomEdoContactsUpdaterService : BackgroundService
	{
		private readonly ILogger<TaxcomEdoContactsUpdaterService> _logger;
		private readonly IZabbixSender _zabbixSender;
		private readonly TaxcomContactsUpdaterOptions _contactsUpdaterOptions;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISettingsController _settingsController;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IEdoContactStateCodeConverter _edoContactStateCodeConverter;
		private readonly ICounterpartyRepository _counterpartyRepository;

		public TaxcomEdoContactsUpdaterService(
			ILogger<TaxcomEdoContactsUpdaterService> logger,
			IUserService userService,
			IZabbixSender zabbixSender,
			IOptions<TaxcomContactsUpdaterOptions> contactsUpdaterOptions,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISettingsController settingsController,
			IServiceScopeFactory serviceScopeFactory,
			IEdoContactStateCodeConverter edoContactStateCodeConverter,
			ICounterpartyRepository counterpartyRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_contactsUpdaterOptions = (contactsUpdaterOptions ?? throw new ArgumentNullException(nameof(contactsUpdaterOptions))).Value;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_edoContactStateCodeConverter =
				edoContactStateCodeConverter ?? throw new ArgumentNullException(nameof(edoContactStateCodeConverter));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Процесс обновления контактов запущен");
			var lastCheckContactsUpdates = _settingsController.GetValue<DateTime>("last_check_contacts_updates");
			await StartWorkingAsync(cancellationToken, lastCheckContactsUpdates);
		}

		private async Task StartWorkingAsync(CancellationToken cancellationToken, DateTime lastCheckContactsUpdates)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				await DelayAsync(cancellationToken);

				try
				{
					_logger.LogInformation("Обновление списка контактов...");

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис обработки контактов ЭДО"))
					{
						var contactUpdates = new EdoContactList();

						do
						{
							using var scope = _serviceScopeFactory.CreateScope();
							var taxcomApiClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
							
							try
							{
								_logger.LogInformation("Получаем список...");
								
								contactUpdates = await taxcomApiClient.GetContactListUpdates(
									lastCheckContactsUpdates, null, cancellationToken);
							}
							catch(Exception e)
							{
								const string errorMessage = "Ошибка при запросе списка контактов";
								_logger.LogError(e, errorMessage);
							}
							
							await _zabbixSender.SendIsHealthyAsync(cancellationToken);

							if(contactUpdates.Contacts is null)
							{
								break;
							}

							foreach(var contact in contactUpdates.Contacts)
							{
								IList<Counterparty> counterparties;

								switch(contact.State.Code)
								{
									case EdoContactStateCode.Incoming:
										await TryAcceptIncomingInvite(contact, uow, taxcomApiClient, cancellationToken);
										break;
									case EdoContactStateCode.Sent:
									case EdoContactStateCode.Accepted:
									case EdoContactStateCode.Rejected:
									case EdoContactStateCode.Error:
										_logger.LogInformation("Обрабатываем контакт в статусе {StateCode}", contact.State.Code);
										counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

										if(counterparties is null)
										{
											break;
										}

										var consentForEdoStatus =
											_edoContactStateCodeConverter.ConvertStateToConsentForEdoStatus(contact.State.Code);
										
										foreach(var counterparty in counterparties)
										{
											if(counterparty.ConsentForEdoStatus == consentForEdoStatus)
											{
												continue;
											}

											_logger.LogInformation(
												"Обновляем согласие на ЭДО у клиента Id {CounterpartyId}" +
												" с {CounterpartyConsentForEdoStatus} на {ConsentForEdoStatus}",
												counterparty.Id,
												counterparty.ConsentForEdoStatus,
												consentForEdoStatus);
											
											if(consentForEdoStatus == ConsentForEdoStatus.Rejected)
											{
												if(counterparty.PersonalAccountIdInEdo != contact.EdxClientId)
												{
													_logger.LogInformation(
														"Пришел отказ на ЭДО у клиента Id {CounterpartyId}" +
														" по кабинету {EdoAccountId}," +
														" хотя у клиента {CounterpartyPersonalAccountIdInEdo} пропускаем...",
														counterparty.Id,
														contact.EdxClientId,
														counterparty.PersonalAccountIdInEdo);
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
											await uow.SaveAsync(counterparty);
											await uow.CommitAsync();
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
					const string errorMessage = "Ошибка при обновлении списка контактов";
					_logger.LogError(e, errorMessage);
				}
				finally
				{
					_settingsController.CreateOrUpdateSetting("last_check_contacts_updates", $"{lastCheckContactsUpdates:s}");
				}
			}
		}

		private async Task TryAcceptIncomingInvite(
			EdoContactInfo contact,
			IUnitOfWork uow,
			ITaxcomApiClient taxcomApiClient,
			CancellationToken cancellationToken)
		{
			IList<Counterparty> counterparties;
			_logger.LogInformation(
				"Входящее приглашение от клиента с аккаунтом {EdxClientId}...", contact.EdxClientId);

			try
			{
				await taxcomApiClient.AcceptContact(contact.EdxClientId, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось принять входящее приглашение от клиента с аккаунтом {EdxClientId}...",
					contact.EdxClientId);
				return;
			}

			counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

			if(counterparties == null)
			{
				return;
			}

			foreach(var counterparty in counterparties)
			{
				_logger.LogInformation("Обновляем данные у клиента Id {CounterpartyId}", counterparty.Id);
				counterparty.EdoOperator =
					_counterpartyRepository.GetEdoOperatorByCode(uow, contact.EdxClientId[..3]);
				counterparty.PersonalAccountIdInEdo = contact.EdxClientId;
				counterparty.ConsentForEdoStatus = ConsentForEdoStatus.Agree;

				await uow.SaveAsync(counterparty);
				await uow.CommitAsync();
			}
		}

		private async Task DelayAsync(CancellationToken cancellationToken)
		{
			var delay = _contactsUpdaterOptions.DelayBetweenContactsProcessingInSeconds;
			
			_logger.LogInformation("Ждем {Delay}сек", delay);
			await Task.Delay(delay * 1000, cancellationToken);
		}
	}
}
