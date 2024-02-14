using Microsoft.AspNetCore.Mvc;
using Pacs.Admin.Server;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Mango.Api.Controllers
{
	[ApiController]
	[Route("pacs/settings")]
	public class SettingsController : ControllerBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ISettingsNotifier _notifier;

		public SettingsController(IUnitOfWorkFactory uowFactory, ISettingsNotifier notifier)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
		}

		[HttpPost("set")]
		public async Task Set([FromBody] SettingsRequest settingsRequest)
		{
			var settings = new DomainSettings
			{
				AdministratorId = settingsRequest.AdministratorId,
				OperatorsOnLongBreak = settingsRequest.OperatorsOnLongBreak,
				LongBreakDuration = settingsRequest.LongBreakDuration,
				LongBreakCountPerDay = settingsRequest.LongBreakCountPerDay,
				OperatorsOnShortBreak = settingsRequest.OperatorsOnShortBreak,
				ShortBreakDuration = settingsRequest.ShortBreakDuration,
				ShortBreakInterval = settingsRequest.ShortBreakInterval,
			};

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				await uow.SaveAsync(settings);
				await uow.CommitAsync();
			}

			await _notifier.SettingsChanged(settings);
		}

		[HttpGet("get")]
		public async Task<DomainSettings> Get()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var settings = await uow.Session.QueryOver<DomainSettings>()
					.OrderBy(x => x.Id).Desc
					.Take(1)
					.SingleOrDefaultAsync();
				return settings;
			}
		}
	}
}
