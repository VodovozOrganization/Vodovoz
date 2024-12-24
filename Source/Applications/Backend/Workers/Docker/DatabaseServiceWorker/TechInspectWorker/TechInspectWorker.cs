using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Logistics;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	internal partial class TechInspectWorker : TimerBackgroundServiceBase
	{
		private bool _workInProgress;
		private readonly ILogger<TechInspectWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<TechInspectOptions> _options;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IZabbixSender _zabbixSender;

		public TechInspectWorker(
			ILogger<TechInspectWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<TechInspectOptions> options,
			ICarEventSettings carEventSettings,
			IZabbixSender zabbixSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			Interval = _options.Value.Interval;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
				nameof(TechInspectWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(TechInspectWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			try
			{
				UpdateLeftUntilTechInspect(DateTime.Now, _carEventSettings.TechInspectCarEventTypeId);
				await _zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при обновлении км до ТО {ErrorDateTime}",
					DateTimeOffset.Now);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' перед следующим запуском", nameof(TechInspectWorker), Interval);

			await Task.CompletedTask;
		}

		private void UpdateLeftUntilTechInspect(DateTime date, int techInspectCarEventTypeId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(TechInspectWorker)))
			{
				using(var transaction = uow.Session.BeginTransaction())
				{
					try
					{
						TryUpdateLeftUntilTechInspect(uow, transaction, techInspectCarEventTypeId);
					}
					catch(Exception ex)
					{
						if(!transaction.WasCommitted
						   && !transaction.WasRolledBack
						   && transaction.IsActive
						   && uow.Session.Connection.State == ConnectionState.Open)
						{
							try
							{
								transaction.Rollback();
							}
							catch { }
						}

						transaction.Dispose();

						_logger.LogError(ex, "Произошла ошибка во время записи пробега, оставшегося до ТО {Message}.", ex.Message);
					}
				}
			}
		}

		private void TryUpdateLeftUntilTechInspect(IUnitOfWork uow, ITransaction transaction, int techInspectCarEventTypeId)
		{
			var carsLeftUntilTechInspect =
				(
					from c in uow.Session.Query<Car>()

					let lastOdometerReadingValue = (int?)
						(
							from or in uow.Session.Query<OdometerReading>()
							where or.Car.Id == c.Id
							orderby or.StartDate descending
							select or.Odometer
						).FirstOrDefault()

					let lastOdometerReadingDate = (DateTime?)
						(
							from or in uow.Session.Query<OdometerReading>()
							where or.Car.Id == c.Id
							orderby or.StartDate descending
							select or.StartDate
						).FirstOrDefault()

					let lastOdometerFromEvent = (int?)
						(
							from ce in uow.GetAll<CarEvent>()
							where ce.Car.Id == c.Id && ce.CarEventType.Id == techInspectCarEventTypeId
							orderby ce.StartDate descending
							select ce.Odometer
						).FirstOrDefault()

					let techInspectInterval = (int?)c.CarModel.TeсhInspectInterval

					let confirmedDistance = (decimal?)
						(
							from rl in uow.GetAll<RouteList>()
							where rl.Car.Id == c.Id && rl.Date >= lastOdometerReadingDate
							select rl.ConfirmedDistance
						).Sum()

					let isTechInspectForKmManual = c.TechInspectForKm != null

					let techInspectForKm = isTechInspectForKmManual
						? c.TechInspectForKm
						: (lastOdometerFromEvent ?? 0) + (techInspectInterval ?? 0)

					let leftUntilTechInspect = techInspectForKm - (lastOdometerReadingValue ?? 0) - (confirmedDistance ?? 0)

					select new
					{
						Car = c,
						LeftUntilTechInspect = leftUntilTechInspect,
					}
				).ToList();

			carsLeftUntilTechInspect.ForEach(x => x.Car.LeftUntilTechInspect = (int)x.LeftUntilTechInspect);

			transaction.Commit();
		}
	}
}
