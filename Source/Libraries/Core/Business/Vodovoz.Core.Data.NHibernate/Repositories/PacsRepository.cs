﻿using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	public class PacsRepository : IPacsRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public PacsRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<Call> GetActiveCalls()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Call callAlias = null;

				var result = uow.Session.QueryOver(() => callAlias)
					.Where(() => callAlias.Status != CallStatus.Disconnected)
					.List();

				return result;
			}
		}

		public IEnumerable<Call> GetCalls(DateTime from)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Call callAlias = null;

				var result = uow.Session.QueryOver(() => callAlias)
					.Where(() => callAlias.CreationTime >= from)
					.List();

				return result;
			}
		}

		public IEnumerable<string> GetAvailablePhones()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				InnerPhone internalPhoneAlias = null;
				var result = uow.Session.QueryOver(() => internalPhoneAlias)
					.Select(Projections.Property(() => internalPhoneAlias.PhoneNumber))
					.List<string>();

				return result;

			}
		}

		public IEnumerable<OperatorState> GetOnlineOperators()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				OperatorState operatorStateAlias = null;
				OperatorSession operatorSessionAlias = null;

				var result = uow.Session.QueryOver(() => operatorStateAlias)
					.Left.JoinAlias(() => operatorStateAlias.Session, () => operatorSessionAlias)
					.Where(() => operatorSessionAlias.Ended == null)
					.List();

				return result;
			}
		}

		public IEnumerable<OperatorState> GetOperators(DateTime from)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				OperatorState operatorStateAlias = null;
				OperatorSession operatorSessionAlias = null;

				var result = uow.Session.QueryOver(() => operatorStateAlias)
					.Left.JoinAlias(() => operatorStateAlias.Session, () => operatorSessionAlias)
					.Where(() => operatorSessionAlias.Started >= from)
					.List();

				return result;
			}
		}

		public IEnumerable<OperatorState> GetOperatorsOnBreak(DateTime date)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				OperatorState stateAlias = null;

				var result = uow.Session.QueryOver(() => stateAlias)
					.Where(() => stateAlias.State == OperatorStateType.Break)
					.Where(() => stateAlias.Started > date)
					.List();
				return result;
			}
		}

		public IEnumerable<OperatorState> GetOperatorBreakStates(int operatorId, DateTime date)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				OperatorState stateAlias = null;

				var result = uow.Session.QueryOver(() => stateAlias)
					.Where(() => stateAlias.State == OperatorStateType.Break)
					.Where(() => stateAlias.Started > date)
					.Where(() => stateAlias.OperatorId == operatorId)
					.List();
				return result;
			}
		}

		public DomainSettings GetPacsDomainSettings()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<DomainSettings>()
					.OrderBy(x => x.Id).Desc
					.Take(1)
					.SingleOrDefault();
				return result;
			}
		}

		public bool PacsEnabledFor(int employeeId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var oper = uow.GetById<Operator>(employeeId);
				return oper != null && oper.PacsEnabled;
			}
		}

		public async Task<Call> GetCallByEntryAsync(IUnitOfWork uow, string entryId)
		{
			return await uow.Session.GetAsync<Call>(entryId);
		}
	}
}
