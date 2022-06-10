using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Utilities.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Roboats;

namespace RoboAtsService.Monitoring
{
	public class RoboatsCallRegistrator
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public RoboatsCallRegistrator(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public RoboatsCall RegisterCall(string phone)
		{
			phone = NormalizePhone(phone);
			var currentCall = GetCurrentCall(phone);
			return currentCall;
		}

		public void RegisterFail(string phone, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			var call = RegisterCall(phone);
			call.Status = RoboatsCallStatus.Fail;
			call.Result = RoboatsCallResult.Nothing;

			var callDetail = new RoboatsCallDetail
			{
				Call = call,
				OperationTime = DateTime.Now,
				FailType = failType,
				Operation = operation,
				Description = description
			};

			call.CallDetails.Add(callDetail);

			uow.Save(call);
			uow.Commit();
		}

		public void RegisterTerminatingFail(string phone, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			var call = RegisterCall(phone);
			call.Status = RoboatsCallStatus.Aborted;
			call.Result = RoboatsCallResult.Nothing;

			var callDetail = new RoboatsCallDetail
			{
				Call = call,
				OperationTime = DateTime.Now,
				FailType = failType,
				Operation = operation,
				Description = description
			};

			call.CallDetails.Add(callDetail);

			uow.Save(call);
			uow.Commit();
		}

		public void RegisterAborted(string phone, RoboatsCallOperation operation, string description = null)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			var call = RegisterCall(phone);
			call.Status = RoboatsCallStatus.Aborted;
			call.Result = RoboatsCallResult.OrderCreated;

			var callDetail = new RoboatsCallDetail
			{
				Call = call,
				OperationTime = DateTime.Now,
				Operation = operation,
				Description = description
			};

			call.CallDetails.Add(callDetail);

			uow.Save(call);
			uow.Commit();
		}

		public void RegisterSuccess(string phone, string description = null)
		{
			using var uow = _uowFactory.CreateWithoutRoot();

			var call = RegisterCall(phone);
			call.Status = RoboatsCallStatus.Success;
			call.Result = RoboatsCallResult.OrderAccepted;

			var callDetail = new RoboatsCallDetail
			{
				Call = call,
				OperationTime = DateTime.Now,
				Operation = RoboatsCallOperation.CreateOrder,
				Description = description
			};

			call.CallDetails.Add(callDetail);

			uow.Save(call);
			uow.Commit();
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
				return CreateNewCall(phone);
			}
			else if(currentCall.CallTime > DateTime.Now.AddMinutes(-10))
			{
				activeCalls.Remove(currentCall);
			}
			else
			{
				currentCall = CreateNewCall(phone);
			}

			CloseStaleCalls(activeCalls);
			return currentCall;
		}

		private RoboatsCall CreateNewCall(string phone)
		{
			return new RoboatsCall
			{
				Phone = phone,
				CallTime = DateTime.Now
			};
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
	}
}
