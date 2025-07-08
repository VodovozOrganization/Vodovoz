using System;
using System.Collections.Generic;
using System.Linq;
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
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Domain.Client;

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
		private readonly IOrganizationRepository _organizationRepository;

		public TaxcomEdoContactsUpdaterService(
			ILogger<TaxcomEdoContactsUpdaterService> logger,
			IUserService userService,
			IZabbixSender zabbixSender,
			IOptions<TaxcomContactsUpdaterOptions> contactsUpdaterOptions,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISettingsController settingsController,
			IServiceScopeFactory serviceScopeFactory,
			IEdoContactStateCodeConverter edoContactStateCodeConverter,
			ICounterpartyRepository counterpartyRepository,
			IOrganizationRepository organizationRepository)
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
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
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
						var organization =
							_organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, _contactsUpdaterOptions.EdoAccount);

						if(organization is null)
						{
							_logger.LogError(
								"Не найдена организация с аккаунтом {EdoAccount} в Такском",
								_contactsUpdaterOptions.EdoAccount);
							
							continue;
						}

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
										await TryAcceptIncomingInvite(contact, organization, uow, taxcomApiClient, cancellationToken);
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
											var edoAccount = counterparty.EdoAccount(organization.Id, contact.EdxClientId);

											if(edoAccount is null)
											{
												_logger.LogWarning(
													"Не нашли аккаунт {ContactEdoAccount} у клиента {Counterparty} для {Organization}",
													contact.EdxClientId,
													counterparty.Name,
													organization.Name
													);
												continue;
											}
											
											if(edoAccount.ConsentForEdoStatus == consentForEdoStatus)
											{
												continue;
											}

											_logger.LogInformation(
												"Обновляем согласие на ЭДО у клиента Id {CounterpartyId}" +
												" с {CounterpartyConsentForEdoStatus} на {ConsentForEdoStatus}",
												counterparty.Id,
												edoAccount.ConsentForEdoStatus,
												consentForEdoStatus);
											
											if(consentForEdoStatus == ConsentForEdoStatus.Agree)
											{
												edoAccount.PersonalAccountIdInEdo = contact.EdxClientId;
												edoAccount.EdoOperator =
													_counterpartyRepository.GetEdoOperatorByCode(uow, contact.EdxClientId[..3]);
											}
											
											edoAccount.ConsentForEdoStatus = consentForEdoStatus;
											await uow.SaveAsync(edoAccount, cancellationToken: cancellationToken);
											await uow.CommitAsync(cancellationToken);
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
			Organization organization,
			IUnitOfWork uow,
			ITaxcomApiClient taxcomApiClient,
			CancellationToken cancellationToken)
		{
			IList<Counterparty> counterparties;
			_logger.LogInformation(
				"Входящее приглашение от клиента с аккаунтом {EdxClientId}...", contact.EdxClientId);

			counterparties = _counterpartyRepository.GetCounterpartiesByINN(uow, contact.Inn);

			if(counterparties == null || !counterparties.Any() || string.IsNullOrWhiteSpace(contact.EdxClientId))
			{
				_logger.LogError(
					"Получено входящее приглашение от несуществующего клиента, ИНН {Inn} аккаунт {EdxClientId} пропускаем...",
					contact.Inn,
					contact.EdxClientId);
				return;
			}

			const string dontAcceptMessage = "Не удалось принять входящее приглашение от клиента, ИНН {Inn} аккаунт {EdxClientId}...";
			
			try
			{
				var result = await taxcomApiClient.AcceptContact(contact.EdxClientId, cancellationToken);

				if(!result)
				{
					_logger.LogError(dontAcceptMessage, contact.Inn, contact.EdxClientId);
					return;
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, dontAcceptMessage, contact.Inn, contact.EdxClientId);
				return;
			}

			foreach(var counterparty in counterparties)
			{
				_logger.LogInformation("Обновляем данные у клиента Id {CounterpartyId}", counterparty.Id);
				
				var edoAccount = counterparty.EdoAccount(organization.Id, contact.EdxClientId)
					?? CounterpartyEdoAccount.Create(
						counterparty,
						_counterpartyRepository.GetEdoOperatorByCode(uow, contact.EdxClientId[..3]),
						contact.EdxClientId,
						organization.Id,
						counterparty.DefaultEdoAccount(organization.Id) is null,
						ConsentForEdoStatus.Agree
					);

				await uow.SaveAsync(edoAccount, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
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
