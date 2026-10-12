using Microsoft.AspNetCore.Http;
using QS.Project.DB;
using System;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	/// <summary>
	/// Построение ключа записи идемпотентного запроса в хранилище:
	/// <c>idempotency:{база}:{API}:{пользователь}:{путь в нижнем регистре}{query string}:{ключ идемпотентности}</c>.
	/// База разводит записи стендов с общим Garnet, API и пользователь - записи разных клиентов с совпавшим ключом
	/// </summary>
	internal sealed class IdempotencyKeyBuilder
	{
		private const string _keyPrefix = "idempotency";

		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;
		private readonly string _apiName;

		public IdempotencyKeyBuilder(IDatabaseConnectionSettings databaseConnectionSettings, string apiName)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));

			if(string.IsNullOrWhiteSpace(apiName))
			{
				throw new ArgumentException("Не указано имя API", nameof(apiName));
			}

			_apiName = apiName;
		}

		/// <summary>
		/// Построение ключа записи
		/// </summary>
		/// <param name="context">Контекст запроса (путь, query string, пользователь)</param>
		/// <param name="idempotencyKey">Ключ идемпотентности из заголовка запроса</param>
		/// <returns>Ключ записи в хранилище</returns>
		public string Build(HttpContext context, Guid idempotencyKey)
		{
			var path = context.Request.Path.Value?.ToLowerInvariant();
			var queryString = context.Request.QueryString.Value;

			return $"{_keyPrefix}:{_databaseConnectionSettings.DatabaseName}:{_apiName}:{context.User.Identity.Name}:"
				+ $"{path}{queryString}:{idempotencyKey:D}";
		}
	}
}
