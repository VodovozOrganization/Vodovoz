using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;

namespace RoboatsService.Monitoring
{
	public class RoboatsCallRegistrator
	{
		private readonly ILogger<RoboatsCallRegistrator> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly RoboatsCallBatchRegistrator _roboatsCallBatchRegistrator;

		public RoboatsCallRegistrator(
			ILogger<RoboatsCallRegistrator> logger,
			IUnitOfWorkFactory uowFactory,
			RoboatsCallBatchRegistrator roboatsCallBatchRegistrator,
			IRoboatsCallFactory roboatsCallFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_roboatsCallBatchRegistrator = roboatsCallBatchRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallBatchRegistrator));
		}

		public void RegisterCall(string phone, Guid callGuid)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = _roboatsCallBatchRegistrator.GetActualCall(uow, phone, callGuid);
				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterFail(string phone, Guid callGuid, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				_roboatsCallBatchRegistrator.RegisterFail(uow, phone, callGuid, failType, operation, description);

				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterTerminatingFail(string phone, Guid callGuid, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				_roboatsCallBatchRegistrator.RegisterTerminatingFail(uow, phone, callGuid, failType, operation, description);

				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void AbortCall(string phone, Guid callGuid)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				_roboatsCallBatchRegistrator.AbortCall(uow, phone, callGuid);

				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterAborted(string phone, Guid callGuid, RoboatsCallOperation operation, string description = null)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				_roboatsCallBatchRegistrator.RegisterAborted(uow, phone, callGuid, operation, description);

				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterSuccess(string phone, Guid callGuid, string description = null)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				_roboatsCallBatchRegistrator.RegisterSuccess(uow, phone, callGuid, description);

				uow.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}
	}
}
