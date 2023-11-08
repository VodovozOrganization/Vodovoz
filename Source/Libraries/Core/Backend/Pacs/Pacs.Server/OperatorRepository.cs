using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorRepository : IOperatorRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public OperatorRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public OperatorState GetOperatorState(int operatorId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Operator operatorAlias = null;
				OperatorState operatorStateAlias = null;

				var result = uow.Session.QueryOver(() => operatorAlias)
					.Inner.JoinAlias(() => operatorAlias.State, () => operatorStateAlias)
					.Where(() => operatorAlias.Id == operatorId)
					.Select(Projections.Entity(() => operatorStateAlias))
					.SingleOrDefault<OperatorState>();

				return result;
			}
		}
	}
}
