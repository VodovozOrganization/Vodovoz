using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;
using Vodovoz.Parameters;

namespace RoboAtsService.Monitoring
{
	public class RoboatsCallRegistrator
	{
		private readonly ILogger<RoboatsCallRegistrator> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IRoboatsCallFactory _roboatsCallFactory;
		private readonly RoboatsSettings _roboatsSettings;

		public RoboatsCallRegistrator(ILogger<RoboatsCallRegistrator> logger, IUnitOfWorkFactory uowFactory, IRoboatsCallFactory roboatsCallFactory, RoboatsSettings roboatsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_roboatsCallFactory = roboatsCallFactory ?? throw new ArgumentNullException(nameof(roboatsCallFactory));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public void RegisterCall(string phone, Guid callGuid)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone, callGuid);
				Save(uow, call);
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

				var call = GetActualCall(phone, callGuid);
				call.Status = RoboatsCallStatus.Fail;
				call.Result = RoboatsCallResult.Nothing;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, failType, operation, description);
				call.CallDetails.Add(callDetail);

				Save(uow, call);
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

				var call = GetActualCall(phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				call.Result = RoboatsCallResult.Nothing;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, failType, operation, description);
				call.CallDetails.Add(callDetail);

				Save(uow, call);
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

				var call = GetActualCall(phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				Save(uow, call);
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

				var call = GetActualCall(phone, callGuid);
				call.Status = RoboatsCallStatus.Aborted;
				call.Result = RoboatsCallResult.OrderCreated;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, operation, description);
				call.CallDetails.Add(callDetail);

				Save(uow, call);
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

				var call = GetActualCall(phone, callGuid);
				call.Status = RoboatsCallStatus.Success;
				call.Result = RoboatsCallResult.OrderAccepted;

				var callDetail = _roboatsCallFactory.GetNewRoboatsCallDetail(call, RoboatsCallOperation.CreateOrder, description);
				call.CallDetails.Add(callDetail);

				Save(uow, call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		private RoboatsCall GetActualCall(string phone, Guid callGuid)
		{
			phone = NormalizePhone(phone);
			var currentCall = GetCurrentCall(phone, callGuid);
			return currentCall;
		}

		private string NormalizePhone(string phone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			return phoneFormatter.FormatString(phone);
		}

		private RoboatsCall GetCurrentCall(string phone, Guid callGuid)
		{
			var call = GetCallByUUID(callGuid);
			if(call == null)
			{
				call = _roboatsCallFactory.GetNewRoboatsCall(phone, callGuid); ;
			}
			return call;
		}

		private RoboatsCall GetCallByUUID(Guid callGuid)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			RoboatsCall roboatsCallAlias = null;
			var call = uow.Session.QueryOver(() => roboatsCallAlias)
				.Where(() => roboatsCallAlias.CallGuid == callGuid)
				.SingleOrDefault();

			return call;
		}

		public void CloseStaleCalls()
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			RoboatsCall roboatsCallAlias = null;
			var staleCalls = uow.Session.QueryOver(() => roboatsCallAlias)
				.Where(() => roboatsCallAlias.CallTime < DateTime.Now.AddMinutes(-_roboatsSettings.CallTimeout))
				.Where(() => roboatsCallAlias.Status == RoboatsCallStatus.InProgress)
				.List();

			foreach(var call in staleCalls)
			{
				var closeDetail = new RoboatsCallDetail
				{
					Call = call,
					Description = $"Закрыт по превышению таймаута ({_roboatsSettings.CallTimeout} мин)",
					FailType = RoboatsCallFailType.TimeOut,
					OperationTime = DateTime.Now,
					Operation = RoboatsCallOperation.ClosingStaleCalls
				};

				call.CallDetails.Add(closeDetail);
				call.Status = RoboatsCallStatus.Aborted;
				call.Result = RoboatsCallResult.Nothing;

				uow.Save(call);
			}
			uow.Commit();
		}

		private void Save(IUnitOfWork uow, RoboatsCall call)
		{
			uow.Save(call);
			uow.Commit();
		}
	}
}
