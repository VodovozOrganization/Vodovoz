using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Zabbix.Sender;

namespace FastDeliveryLateWorker
{
	public class FastDeliveryLateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<FastDeliveryLateWorker> _logger;
		private readonly IOptions<FastDeliveryLateOptions> _options;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private bool _workInProgress;

		public FastDeliveryLateWorker(
			ILogger<FastDeliveryLateWorker> logger,
			IOptions<FastDeliveryLateOptions> options,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
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
				using var scope = _serviceScopeFactory.CreateScope();

				CreateComplaintsForFasteDeliveryLateOrders(scope.ServiceProvider);

				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();
				await zabbixSender.SendIsHealthyAsync(stoppingToken);
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

		private void CreateComplaintsForFasteDeliveryLateOrders(IServiceProvider serviceProvider)
		{
			var unitOfWorkFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();

			using var uow = unitOfWorkFactory.CreateWithoutRoot(nameof(FastDeliveryLateWorker));
			
			var deliveryRepository = serviceProvider.GetRequiredService<IDeliveryRepository>();
			var generalSettings = serviceProvider.GetRequiredService<IGeneralSettings>();

			var fastDeliveryLateOrders = deliveryRepository.GetFastDeliveryLateOrders(uow, DateTime.Today, generalSettings, _options.Value.ComplaintDetalizationId);

			if(!fastDeliveryLateOrders.Any())
			{
				return;
			}

			var complaintDetalizationRepository = serviceProvider.GetRequiredService<IGenericRepository<ComplaintDetalization>>();

			var complaintDetalization = complaintDetalizationRepository.Get(uow, cd => cd.Id == _options.Value.ComplaintDetalizationId).FirstOrDefault();

			var employeeRepository = serviceProvider.GetRequiredService<IEmployeeRepository>();

			var currentEmployee = employeeRepository.GetEmployeeForCurrentUser(uow);

			foreach(var lateOrder in fastDeliveryLateOrders)
			{
				var isPrepayment = lateOrder.PaymentType == PaymentType.PaidOnline || lateOrder.PaymentType == PaymentType.SmsQR;

				var isSouthDistrict = lateOrder.DeliveryPoint.District.GeographicGroup.Id == _options.Value.SouthGeoGroupId;

				var complaintText = "Не исполнены условия Экспресс доставки.\n"
						+ (isPrepayment
						? "Осуществить возврат средств за экспресс- доставку."
						: "");

				var complaint = new Complaint
				{
					Order = lateOrder,
					Counterparty = lateOrder.Client,
					DeliveryPoint = lateOrder.DeliveryPoint,
					ComplaintKind = complaintDetalization.ComplaintKind,
					ComplaintDetalization = complaintDetalization,
					ComplaintSource = new ComplaintSource { Id = _options.Value.ComplaintSourceId },
					ComplaintText = complaintText,
					ComplaintType = ComplaintType.Client,
					CreationDate = DateTime.Now,
					ChangedDate = DateTime.Now,
					CreatedBy = currentEmployee,
					ChangedBy = currentEmployee
				};

				var nomenclatureSettings = serviceProvider.GetRequiredService<INomenclatureSettings>();
				var orderSettings = serviceProvider.GetRequiredService<IOrderSettings>();

				var fastDeliveryOrderItem = lateOrder.OrderItems.FirstOrDefault(x => x.Nomenclature.Id == nomenclatureSettings.FastDeliveryNomenclatureId);
				fastDeliveryOrderItem.SetDiscount(100);
				fastDeliveryOrderItem.DiscountReason = new DiscountReason { Id = orderSettings.FastDeliveryLateDiscountReasonId };
				uow.Save(fastDeliveryOrderItem);

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
