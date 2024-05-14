using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
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
		private readonly IUnitOfWork _uow;
		private readonly ComplaintDetalization _complaintDetalization;
		private readonly Employee _currentEmployee;

		private bool _workInProgress;

		public FastDeliveryLateWorker(
			ILogger<FastDeliveryLateWorker> logger,
			IOptions<FastDeliveryLateOptions> options,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeneralSettings generalSettings,
			IDeliveryRepository deliveryRepository,
			IEmployeeRepository employeeRepository)
		{
			if(employeeRepository is null)
			{
				throw new ArgumentNullException(nameof(employeeRepository));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));

			_uow = _unitOfWorkFactory.CreateWithoutRoot((nameof(FastDeliveryLateWorker)));
			_complaintDetalization = _uow.Session.Get<ComplaintDetalization>(_options.Value.ComplaintDetalizationId);
			_currentEmployee = employeeRepository.GetEmployeeForCurrentUser(_uow);
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
			var fastDeliveryLateOrders = _deliveryRepository.GetFastDeliveryLateOrders(_uow, DateTime.Today, _generalSettings, _complaintDetalization.Id);

			if(!fastDeliveryLateOrders.Any())
			{
				return;
			}

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
					ComplaintKind = _complaintDetalization.ComplaintKind,
					ComplaintDetalization = _complaintDetalization,
					ComplaintSource = new ComplaintSource { Id = _options.Value.ComplaintSourceId },
					ComplaintText = complaintText,
					ComplaintType = ComplaintType.Client,
					CreationDate = DateTime.Now,
					ChangedDate = DateTime.Now,
					CreatedBy = _currentEmployee,
					ChangedBy = _currentEmployee,
				};

				_uow.Save(complaint);

				var guilty = new ComplaintGuiltyItem
				{
					Complaint = complaint,
					Subdivision = new Subdivision { Id = isSouthDistrict ? _options.Value.LoSofiyskayaSubdivisionId : _options.Value.LoBugrySubdivisionId },
					Responsible = new Responsible { Id = _options.Value.ResponsibleId }
				};

				_uow.Save(guilty);

				_uow.Commit();
			}
		}
	}
}
