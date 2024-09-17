using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Taxcom.Client.Api;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Factories;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Settings;

namespace TaxcomEdoApi.Services
{
	public class ContactsUpdaterService : BackgroundService
	{
		private readonly ILogger<ContactsUpdaterService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly EdoServicesOptions _edoServicesOptions;
		private readonly IEdoContactInfoFactory _edoContactInfoFactory;
		private readonly ISettingsController _settingsController;

		public ContactsUpdaterService(
			ILogger<ContactsUpdaterService> logger,
			TaxcomApi taxcomApi,
			IOptions<EdoServicesOptions> edoServicesOptions,
			IEdoContactInfoFactory edoContactInfoFactory,
			ISettingsController settingsController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_edoServicesOptions = (edoServicesOptions ?? throw new ArgumentNullException(nameof(edoServicesOptions))).Value;
			_edoContactInfoFactory = edoContactInfoFactory ?? throw new ArgumentNullException(nameof(edoContactInfoFactory));
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
							var contactInfo =
								_edoContactInfoFactory.CreateEdoContactInfo(
									contact.EdxClientId,
									contact.Inn,
									contact.State.Code.ToString());
							
							//TODO отправляем сообщение в пулл
							

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
			var delay = _edoServicesOptions.DelayBetweenContactsProcessingInSeconds;
			
			_logger.LogInformation("Ждем {DelaySec}сек", delay);
			await Task.Delay(delay * 1000, stoppingToken);
		}
	}
}
