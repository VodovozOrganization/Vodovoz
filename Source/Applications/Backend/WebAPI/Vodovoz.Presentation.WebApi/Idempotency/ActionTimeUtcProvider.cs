using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	internal sealed class ActionTimeUtcProvider : IActionTimeUtcProvider
	{
		// Формат, в котором мобильное приложение передаёт время и в теле запроса: 2026-08-12T13:34:08Z.
		// Дробная часть секунд допускается, но не обязательна
		private static readonly string[] _actionTimeUtcFormats =
		{
			"yyyy-MM-dd'T'HH:mm:ss'Z'",
			"yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'"
		};

		private readonly IHttpContextAccessor _httpContextAccessor;

		public ActionTimeUtcProvider(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		}

		/// <inheritdoc/>
		public bool TryGetActionTimeUtc(out DateTime actionTimeUtc, out string rawHeaderValue)
		{
			actionTimeUtc = default;
			rawHeaderValue = string.Empty;

			var headers = _httpContextAccessor.HttpContext?.Request.Headers;

			if(headers is null
				|| !headers.TryGetValue(IdempotencyRequestHeadersNames.ActionTimeUtc, out var headerValues)
				|| headerValues.Count == 0)
			{
				return false;
			}

			rawHeaderValue = headerValues.ToString();

			return DateTime.TryParseExact(
				rawHeaderValue,
				_actionTimeUtcFormats,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
				out actionTimeUtc);
		}
	}
}
