using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Client.Api;
using TaxcomEdo.Contracts.Counterparties;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Settings;

namespace TaxcomEdoApi.Services
{
	public class ContactsUpdaterService : BackgroundService
	{
		private readonly ILogger<ContactsUpdaterService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly ISettingsController _settingsController;
		private const int _delaySec = 120;

		public ContactsUpdaterService(
			ILogger<ContactsUpdaterService> logger,
			TaxcomApi taxcomApi,
			ISettingsController settingsController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс обновления контактов запущен");
			//нужно организовывать хранение времени последней обработки списка контактов
			var lastCheckContactsUpdates = _settingsController.GetValue<DateTime>("last_check_contacts_updates");
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
							//TODO подумать, как будем поступать с входящими предложениями обмена
							/*case ContactStateCode.Incoming:
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
							}*/
							
							var contactInfo =
								EdoContactInfo.Create(
									contact.EdxClientId,
									contact.Inn,
									contact.State.Code.ToString());
							
							//отправляем сообщение в пулл

							lastCheckContactsUpdates = contact.State.Changed;
						}
					} while(contactUpdates.Contacts != null && contactUpdates.Contacts.Length >= 100);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при обновлении списка контактов");
				}
				finally
				{
					_settingsController.CreateOrUpdateSetting("last_check_contacts_updates", $"{lastCheckContactsUpdates:s}");
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
