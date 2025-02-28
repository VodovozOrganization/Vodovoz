using Microsoft.Extensions.Hosting;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Infrastructure
{
	/// <summary>
	/// Сервис вызывающий подключение к базе данных в момент старта хоста.
	/// </summary>
	public sealed class InitDbConnectionOnHostStartedService : IHostedService
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public InitDbConnectionOnHostStartedService(
			IUnitOfWorkFactory uowFactory,
			IHostApplicationLifetime appLifetime)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			appLifetime.ApplicationStarted.Register(OnStarted);
		}

		private void OnStarted()
		{
			InitilizeDbConnection();
		}

		public void InitilizeDbConnection()
		{
			_uowFactory.CreateWithoutRoot().Dispose();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
