using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mango.Client;
using MangoService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mango.Core;

namespace Mango.Service.HostedServices
{
	public class PhonebookHostedService : PhonebookService.PhonebookServiceBase, IHostedService
	{
		private readonly ILogger<PhonebookHostedService> _logger;
		private readonly MangoController _mangoController;
		private readonly CallsHostedService _callsService;

		private List<PhoneEntry> _phones = new List<PhoneEntry>();

		public PhonebookHostedService(ILogger<PhonebookHostedService> logger, MangoController mangoController, CallsHostedService callsService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoController = mangoController ?? throw new ArgumentNullException(nameof(mangoController));
			_callsService = callsService ?? throw new ArgumentNullException(nameof(callsService));
		}

		#region GRPC

		public override Task<PhoneBook> GetBook(Empty request, ServerCallContext context)
		{
			var book = new PhoneBook();
			foreach(var phone in _phones)
			{
				if(phone.PhoneType == PhoneEntryType.Extension)
				{
					phone.PhoneState = _callsService.Calls.Values.Any(c =>
						(c.LastEvent.To.Extension.ParseExtension() == phone.Extension || c.LastEvent.From.Extension.ParseExtension() == phone.Extension) &&
						c.IsActive)
						? PhoneState.Busy
						: PhoneState.Ready;
				}
				book.Entries.Add(phone);
			}
			return Task.FromResult(book);
		}

		#endregion

		#region Timer
		private Timer _timer;

		private void RefreshPhones(object state)
		{
			try
			{
				RefreshPhones();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обновлении телефонной книги манго");
				if(_phones.Any())
				{
					return;
				}
				_phones.Add(
					new PhoneEntry
					{
						Extension = 0,
						Name = "Телефонная книга пуста",
						Department = string.Empty,
						PhoneType = PhoneEntryType.Extension,
						PhoneState = PhoneState.Busy
					}
				);
			}
		}

		private void RefreshPhones()
		{
			var list = new List<PhoneEntry>();
			foreach(var user in _mangoController.GetAllVPBXUsers())
			{
				if(uint.TryParse(user.telephony.extension, out uint extension))
				{
					list.Add(new PhoneEntry
					{
						Extension = extension,
						Name = user.general.name,
						Department = user.general.department ?? string.Empty,
						PhoneType = PhoneEntryType.Extension
					});
				}
			}

			foreach(var group in _mangoController.GetAllVpbxGroups())
			{
				if(uint.TryParse(group.extension, out uint extension))
				{
					list.Add(new PhoneEntry
					{
						Extension = extension,
						Name = group.name,
						Department = group.descpription ?? string.Empty,
						PhoneType = PhoneEntryType.Group,
						PhoneState = PhoneState.Ready
					});
				}
			}

			_phones = list;
		}
		#endregion

		#region FindPhone

		public PhoneEntry FindPhone(string number)
		{
			if(uint.TryParse(number, out uint extension))
			{
				var phone = _phones.Find(x => x.Extension == extension);
				if(phone == null)
				{
					RefreshPhones(null);
					phone = _phones.Find(x => x.Extension == extension);
				}
				return phone;
			}
			return null;
		}

		#endregion

		#region IHostedService
		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис телефонной книги запущен.");
			_timer = new Timer(RefreshPhones, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Сервис телефонной книги остановлен.");

			_timer?.Change(Timeout.Infinite, 0);

			return Task.CompletedTask;
		}
		#endregion
	}
}
