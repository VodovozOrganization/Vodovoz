using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using System;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.Settings.Roboats;

namespace RoboatsService.Monitoring
{
	public class RoboatsCallBatchRegistrator
	{
		private readonly ILogger<RoboatsCallRegistrator> _logger;
		private readonly IRoboatsCallFactory _roboatsCallFactory;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IRoboatsSettings _roboatsSettings;

		public RoboatsCallBatchRegistrator(
			ILogger<RoboatsCallRegistrator> logger,
			IRoboatsCallFactory roboatsCallFactory,
			IRoboatsRepository roboatsRepository,
			IRoboatsSettings roboatsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsCallFactory = roboatsCallFactory ?? throw new ArgumentNullException(nameof(roboatsCallFactory));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public void RegisterCall(IUnitOfWork uow, string phone, Guid callGuid)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterFail(IUnitOfWork uow, string phone, Guid callGuid, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				call.Status = RoboatsCallStatus.Fail;
				call.Result = RoboatsCallResult.Nothing;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, failType, operation, description);
				call.CallDetails.Add(callDetail);
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterTerminatingFail(IUnitOfWork uow, string phone, Guid callGuid, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				call.Result = RoboatsCallResult.Nothing;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, failType, operation, description);
				call.CallDetails.Add(callDetail);
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void AbortCall(IUnitOfWork uow, string phone, Guid callGuid)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterAborted(IUnitOfWork uow, string phone, Guid callGuid, RoboatsCallOperation operation, string description = null)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				call.Result = RoboatsCallResult.OrderCreated;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, operation, description);
				call.CallDetails.Add(callDetail);
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterSuccess(IUnitOfWork uow, string phone, Guid callGuid, string description = null)
		{
			try
			{
				var call = GetActualCall(uow, phone, callGuid);
				call.Status = RoboatsCallStatus.Success;
				call.Result = RoboatsCallResult.OrderAccepted;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, RoboatsCallOperation.CreateOrder, description);
				call.CallDetails.Add(callDetail);
				uow.Save(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		internal RoboatsCall GetActualCall(IUnitOfWork uow, string phone, Guid callGuid)
		{
			phone = NormalizePhone(phone);
			var currentCall = GetCurrentCall(uow, phone, callGuid);
			return currentCall;
		}

		private string NormalizePhone(string phone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			return phoneFormatter.FormatString(phone);
		}

		private RoboatsCall GetCurrentCall(IUnitOfWork uow, string phone, Guid callGuid)
		{
			var call = GetCallByUUID(uow, callGuid);
			if(call == null)
			{
				call = _roboatsCallFactory.GetNewRoboatsCall(phone, callGuid);
				uow.Save(call);
			}
			return call;
		}

		private RoboatsCall GetCallByUUID(IUnitOfWork uow, Guid callGuid)
		{
			var call = _roboatsRepository.GetCall(uow, callGuid);
			return call;
		}
	}
}
