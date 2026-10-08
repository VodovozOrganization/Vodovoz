using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Idempotency;

namespace DriverAPI.Controllers.V7
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("7.0")]
	[Route("api/v{version:apiVersion}/[action]")]
	[ApiController]
	public class VersionedController : ApiControllerBase
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		public VersionedController(ILogger<ApiControllerBase> logger) : base(logger)
		{
		}

		/// <summary>
		/// Время действия в мобильном приложении, UTC
		/// При включённом флаге <see cref="FeatureFlags.ActionTimeUtcFromHeader"/> - из заголовка X-Action-Time-Utc,
		/// а если его нет или он некорректен - из тела запроса (с предупреждением в логе)
		/// При выключенном флаге - из тела запроса, заголовок не читается
		/// Перевод в локальное время в данном методе не выполняется
		/// </summary>
		/// <param name="bodyActionTimeUtc">Время действия из тела запроса</param>
		/// <returns>Время действия, UTC</returns>
		protected async Task<DateTime> GetActionTimeUtcAsync(DateTime bodyActionTimeUtc)
		{
			var featureManager = HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

			if(!await featureManager.IsEnabledAsync(FeatureFlags.ActionTimeUtcFromHeader))
			{
				return bodyActionTimeUtc;
			}

			var actionTimeUtcProvider = HttpContext.RequestServices.GetRequiredService<IActionTimeUtcProvider>();

			if(actionTimeUtcProvider.TryGetActionTimeUtc(out var headerActionTimeUtc, out var rawHeaderValue))
			{
				return headerActionTimeUtc;
			}

			_logger.LogWarning(
				"Заголовок {HeaderName} не найден или некорректен ({RawValue}), время действия взято из тела запроса: {BodyActionTimeUtc}",
				IdempotencyRequestHeadersNames.ActionTimeUtc,
				rawHeaderValue,
				bodyActionTimeUtc);

			return bodyActionTimeUtc;
		}
	}
}
