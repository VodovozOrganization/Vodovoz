using FuelControl.Library.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library
{
	/// <summary>
	/// Проверяет и при необходимости исправляет заголовки запросов к API Газпрома непосредственно перед отправкой в сеть.<br/>
	/// - заголовки без управляющих символов не изменяются;<br/>
	/// - управляющие символы и пробелы по краям значения удаляются, запрос отправляется с исправленным значением
	/// - если управляющие символы есть внутри значения или после обрезки значение пустое,
	///   запрос не отправляется и выбрасывается <see cref="FuelControlException"/>.<br/>
	/// </summary>
	public class GazpromRequestHeadersValidationHandler : DelegatingHandler
	{
		private readonly ILogger<GazpromRequestHeadersValidationHandler> _logger;

		public GazpromRequestHeadersValidationHandler(ILogger<GazpromRequestHeadersValidationHandler> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			foreach(var header in request.Headers.ToList())
			{
				var values = header.Value.ToList();

				if(!values.Any(ContainsControlChars))
				{
					continue;
				}

				// Удаляем по краям пробелы, табуляции и переносы строк (\r, \n)
				var trimmedValues = values.Select(v => v?.Trim()).ToList();

				// Перенос внутри значения или значение из одних переносов исправить нельзя,
				// поэтому запрос не отправляем, чтобы не получить блокировку IP
				if(trimmedValues.Any(v => ContainsControlChars(v) || string.IsNullOrEmpty(v)))
				{
					_logger.LogError(
						"Запрос {RequestUri} к API Газпром не отправлен: значение заголовка {HeaderName} содержит управляющие символы",
						request.RequestUri,
						header.Key);

					throw new FuelControlException(
						$"Значение заголовка {header.Key} запроса к API Газпром содержит недопустимые символы (переносы строк и т.п.). " +
						"Запрос не отправлен. Проверьте данные для авторизации в API управления топливом в настройках пользователя");
				}

				_logger.LogWarning(
					"Из значения заголовка {HeaderName} запроса {RequestUri} к API Газпром удалены переносы строк и пробелы по краям. " +
					"Проверьте данные для авторизации в API управления топливом в настройках пользователя",
					header.Key,
					request.RequestUri);

				// Заменяем заголовок исправленным значением. Без валидации, т.к. имена заголовков Газпрома (api_key, session_id и т.п.) не стандартные, а значения уже проверены выше
				request.Headers.Remove(header.Key);
				request.Headers.TryAddWithoutValidation(header.Key, trimmedValues);
			}

			return base.SendAsync(request, cancellationToken);
		}

		/// <summary>
		/// Есть ли в строке управляющие символы (переносы строк, табуляции и т.п.)
		/// </summary>
		private static bool ContainsControlChars(string value) =>
			value != null && value.Any(char.IsControl);
	}
}
