using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;

namespace FastDeliveryLateWorker
{
	public class FastDeliveryLateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<FastDeliveryLateWorker> _logger;
		private readonly IOptions<FastDeliveryLateOptions> _options;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGeneralSettings _generalSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<ComplaintDetalization> _complaintDetalizationRepository;
		private bool _workInProgress;

		public FastDeliveryLateWorker(
			ILogger<FastDeliveryLateWorker> logger,
			IOptions<FastDeliveryLateOptions> options,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeneralSettings generalSettings,
			IDeliveryRepository deliveryRepository,
			IEmployeeRepository employeeRepository,
			IGenericRepository<ComplaintDetalization> complaintDetalizationRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_complaintDetalizationRepository = complaintDetalizationRepository ?? throw new ArgumentNullException(nameof(complaintDetalizationRepository));
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {StartTime}",
				nameof(FastDeliveryLateWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(FastDeliveryLateWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval => _options.Value.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			try
			{
				CreateComplaintsForFasteDeliveryLateOrders();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при работе воркера {Worker} {ErrorDateTime}",
					nameof(FastDeliveryLateWorker),
					DateTimeOffset.Now);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' перед следующим запуском", nameof(FastDeliveryLateWorker), Interval);

			await Task.CompletedTask;
		}

		private void CreateComplaintsForFasteDeliveryLateOrders()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot((nameof(FastDeliveryLateWorker))))
			{
				var fastDeliveryLateOrders = _deliveryRepository.GetFastDeliveryLateOrders(uow, DateTime.Today, _generalSettings, _options.Value.ComplaintDetalizationId);

				if(!fastDeliveryLateOrders.Any())
				{
					return;
				}

				var complaintDetalization = _complaintDetalizationRepository.Get(uow, cd => cd.Id == _options.Value.ComplaintDetalizationId).FirstOrDefault();
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

				foreach(var lateOrder in fastDeliveryLateOrders)
				{
					var isPrepayment = lateOrder.PaymentType == PaymentType.PaidOnline || lateOrder.PaymentType == PaymentType.SmsQR;

					var isSouthDistrict = lateOrder.GeoGroupId == _options.Value.SouthGeoGroupId;

					var complaintText = "Не исполнены условия Экспресс доставки.\n"
							+ (isPrepayment
							? "Осуществить возврат средств за экспресс- доставку."
							: "");

					var complaint = new Complaint
					{
						Order = new Order { Id = lateOrder.OrderId },
						DeliveryPoint = new DeliveryPoint { Id = lateOrder.DeliveryPointId },
						ComplaintKind = complaintDetalization.ComplaintKind,
						ComplaintDetalization = complaintDetalization,
						ComplaintSource = new ComplaintSource { Id = _options.Value.ComplaintSourceId },
						ComplaintText = complaintText,
						ComplaintType = ComplaintType.Client,
						CreationDate = DateTime.Now,
						ChangedDate = DateTime.Now,
						CreatedBy = currentEmployee,
						ChangedBy = currentEmployee,
					};

					uow.Save(complaint);

					var guilty = new ComplaintGuiltyItem
					{
						Complaint = complaint,
						Subdivision = new Subdivision { Id = isSouthDistrict ? _options.Value.LoSofiyskayaSubdivisionId : _options.Value.LoBugrySubdivisionId },
						Responsible = new Responsible { Id = _options.Value.ResponsibleId }
					};

					uow.Save(guilty);

					uow.Commit();
				}
			}
		}
	}
}
