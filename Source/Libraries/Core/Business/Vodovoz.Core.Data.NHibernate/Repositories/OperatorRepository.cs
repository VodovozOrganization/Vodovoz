using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	public class OperatorRepository : IOperatorRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public OperatorRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<OperatorState> GetOperatorHistory(int operatorId)
		{
			throw new NotImplementedException();
		}

		public OperatorState GetOperatorState(int operatorId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				OperatorState operatorStateAlias = null;

				var result = uow.Session.QueryOver(() => operatorStateAlias)
					.Where(() => operatorStateAlias.OperatorId == operatorId)
					.OrderBy(() => operatorStateAlias.Id).Desc
					.Take(1)
					.SingleOrDefault<OperatorState>();

				return result;
			}
		}

		public Operator GetOperator(int operatorId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.Get<Operator>(operatorId);
				return result;
			}
		}
	}
}
