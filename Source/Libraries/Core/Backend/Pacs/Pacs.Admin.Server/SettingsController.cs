using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Server
{
	[ApiController]
	[Route("pacs/settings")]
	[Authorize]
	public class SettingsController : ControllerBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ISettingsNotifier _notifier;

		public SettingsController(IUnitOfWorkFactory uowFactory, ISettingsNotifier notifier)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
		}

		[HttpPost]
		[Route("set")]
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

		[HttpGet]
		[Route("get")]
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
