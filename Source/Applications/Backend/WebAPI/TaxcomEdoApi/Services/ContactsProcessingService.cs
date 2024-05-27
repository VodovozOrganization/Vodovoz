using EdoService.Library.Converters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Client.Api;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Data.Clients;

namespace TaxcomEdoApi.Services
{
	public class ContactsProcessingService : BackgroundService
	{
		private readonly ILogger<ContactsUpdaterService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly IContactStateConverter _contactStateConverter;
		private const int _delaySec = 120;

		public ContactsProcessingService(
			ILogger<ContactsUpdaterService> logger,
			TaxcomApi taxcomApi,
			IContactStateConverter contactStateConverter)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_contactStateConverter = contactStateConverter ?? throw new ArgumentNullException(nameof(contactStateConverter));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс обновления контактов запущен");
			//нужно организовывать хранение времени последней обработки списка контактов
			//var lastCheckContactsUpdates = _settingsController.GetValue<DateTime>("last_check_contacts_updates");
			await StartWorkingAsync(stoppingToken, lastCheckContactsUpdates);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken, DateTime lastCheckContactsUpdates)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);

				try
				{
					_logger.LogInformation("Обрабатываем информацию о списке контактов...");
					
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
							var contactInfo =
								EdoContactInfo.Create(
									contact.EdxClientId,
									contact.Inn,
									_contactStateConverter.ConvertStateToEdoContactStateCode(contact.State.Code));
							
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
