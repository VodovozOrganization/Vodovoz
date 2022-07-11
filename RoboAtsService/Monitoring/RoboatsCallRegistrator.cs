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

		public void RegisterCall(string phone)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone);
				Save(uow, call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		public void RegisterFail(string phone, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone);
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

		public void RegisterTerminatingFail(string phone, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone);
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

		public void RegisterAborted(string phone, RoboatsCallOperation operation, string description = null)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone);
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

		public void RegisterSuccess(string phone, string description = null)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot();

				var call = GetActualCall(phone);
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

		private RoboatsCall GetActualCall(string phone)
		{
			phone = NormalizePhone(phone);
			var currentCall = GetCurrentCall(phone);
			return currentCall;
		}

		private string NormalizePhone(string phone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.DigitsTen);
			return phoneFormatter.FormatString(phone);
		}

		private RoboatsCall GetCurrentCall(string phone)
		{
			var activeCalls = LoadActiveCalls(phone).ToList();
			var currentCall = activeCalls.OrderByDescending(x => x.CallTime).FirstOrDefault();
			if(currentCall == null)
			{
				return _roboatsCallFactory.GetNewRoboatsCall(phone);
			}
			else if(currentCall.CallTime > DateTime.Now.AddMinutes(-_roboatsSettings.NewCallTimeout))
			{
				activeCalls.Remove(currentCall);
			}
			else
			{
				currentCall = _roboatsCallFactory.GetNewRoboatsCall(phone);
			}

			CloseStaleCalls(activeCalls);
			return currentCall;
		}

		private IEnumerable<RoboatsCall> LoadActiveCalls(string phone)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			RoboatsCallStatus[] activeStatuses = { RoboatsCallStatus.InProgress, RoboatsCallStatus.Fail };

			RoboatsCall roboatsCallAlias = null;
			var call = uow.Session.QueryOver(() => roboatsCallAlias)
				.Where(() => roboatsCallAlias.Phone == phone)
				.Where(Restrictions.In(Projections.Property(() => roboatsCallAlias.Status), activeStatuses))
				.List();
			return call;
		}

		private void CloseStaleCalls(IEnumerable<RoboatsCall> staleCalls)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			foreach(var call in staleCalls)
			{
				call.Status = RoboatsCallStatus.Aborted;
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
