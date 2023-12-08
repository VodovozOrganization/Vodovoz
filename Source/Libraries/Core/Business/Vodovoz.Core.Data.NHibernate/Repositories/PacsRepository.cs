using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Pacs.Server;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Organizations;
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

		public IEnumerable<CallEvent> GetActiveCalls()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				CallEvent callEventAlias = null;
				CallEvent callEvent2Alias = null;

				var result = uow.Session.QueryOver(() => callEventAlias)
					.JoinEntityAlias(
						() => callEvent2Alias,
						() => callEventAlias.CallId == callEvent2Alias.CallId
							&& callEvent2Alias.CallState == CallState.Disconnected,
						JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => callEvent2Alias.Id).IsNull
					.Where(() => callEventAlias.CreationTime > DateTime.Now.AddDays(-1))
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

		public bool PacsEnabledFor(int subdivisionId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<SubdivisionEntity>()
					.Where(x => x.Id == subdivisionId)
					.Select(x => x.PacsTimeManagementEnabled)
					.List<bool>().FirstOrDefault();
				return result;
			}
		}
	}
}
