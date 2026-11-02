using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Linq;
using System.Net;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;
using Vodovoz.Settings.ResourceLocker;

namespace Vodovoz.Infrastructure.WebApi.Caching.Garnet
{
	/// <summary>
	/// Регистрация подключения к Garnet и хранилищ на нём для Web API
	/// </summary>
	public static class DependencyInjection
	{
		private const int _connectTimeoutMilliseconds = 3000;
		private const int _commandTimeoutMilliseconds = 1000;

		// Одна попытка при создании подключения: по умолчанию их 3, и старт при недоступном Garnet задерживается кратно ConnectTimeout
		// Дальше переподключение идёт в фоне
		private const int _connectRetryCount = 1;

		/// <summary>
		/// Регистрация подключения к Garnet (<see cref="IConnectionMultiplexer"/>, singleton) для Web API
		/// Подключение настроено так, чтобы недоступность Garnet не мешала работе API:
		/// старт не падает, переподключение идёт в фоне, команды при разрыве сразу завершаются ошибкой.<br/>
		/// Требует строк <c>GarnetUrl</c> и <c>GarnetPassword</c> в <c>base_parameters</c>: они читаются
		/// при создании подключения, то есть на старте приложения.<br/>
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddWebApiGarnetConnection(this IServiceCollection services) =>
			services.AddSingleton<IConnectionMultiplexer>(sp =>
			{
				var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DependencyInjection).FullName);
				var garnetSettings = sp.GetRequiredService<IGarnetSettings>();

				var options = ConfigurationOptions.Parse(garnetSettings.ConnectionString);
				options.AbortOnConnectFail = false;
				options.BacklogPolicy = BacklogPolicy.FailFast;
				options.ConnectTimeout = _connectTimeoutMilliseconds;
				options.ConnectRetry = _connectRetryCount;
				options.SyncTimeout = _commandTimeoutMilliseconds;
				options.AsyncTimeout = _commandTimeoutMilliseconds;

				var connection = ConnectionMultiplexer.Connect(options);

				connection.ConnectionFailed += (sender, args) =>
					logger.LogWarning(
						args.Exception,
						"Потеряно подключение к Garnet {EndPoint}: {FailureType}",
						FormatEndPoint(args.EndPoint),
						args.FailureType);

				connection.ConnectionRestored += (sender, args) =>
					logger.LogInformation(
						"Подключение к Garnet {EndPoint} восстановлено",
						FormatEndPoint(args.EndPoint));

				var endpoints = string.Join(", ", options.EndPoints.Select(FormatEndPoint));

				if(connection.IsConnected)
				{
					logger.LogInformation("Подключение к Garnet создано: {Endpoints}", endpoints);
				}
				else
				{
					logger.LogWarning("Garnet недоступен при старте: {Endpoints}, переподключение идёт в фоне", endpoints);
				}

				return connection;
			});

		/// <summary>
		/// Регистрация хранилища записей идемпотентных запросов на Garnet (<see cref="IIdempotencyStore"/>, singleton).
		/// Требует зарегистрированного <see cref="IConnectionMultiplexer"/> (<see cref="AddWebApiGarnetConnection"/>)
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddGarnetIdempotencyStore(this IServiceCollection services) =>
			services.AddSingleton<IIdempotencyStore, GarnetIdempotencyStore>();

		private static string FormatEndPoint(EndPoint endPoint) =>
			endPoint is DnsEndPoint dnsEndPoint
				? $"{dnsEndPoint.Host}:{dnsEndPoint.Port}"
				: endPoint?.ToString();
	}
}
