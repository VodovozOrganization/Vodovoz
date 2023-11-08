using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class PhoneRepository : IPhoneRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public PhoneRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<PhoneAssignment> GetPhoneAssignments()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Operator operatorAlias = null;
				OperatorState operatorStateAlias = null;
				InternalPhone phoneAlias = null;
				PhoneAssignment resultAlias = null;

				var assignments = uow.Session.QueryOver(() => operatorAlias)
					.Inner.JoinAlias(() => operatorAlias.State, () => operatorStateAlias)
					.JoinEntityQueryOver(() => phoneAlias, () => operatorStateAlias.PhoneNumber == phoneAlias.PhoneNumber, JoinType.RightOuterJoin)
					//.Right.JoinAlias(() => operatorStateAlias.PhoneNumber, () => phoneAlias)
					.Where(() => operatorStateAlias.State != OperatorStateType.Disconnected)
					.WhereRestrictionOn(() => operatorStateAlias.PhoneNumber).IsNotNull()
					.SelectList(list => list
						.Select(Projections.Property(() => phoneAlias.PhoneNumber)).WithAlias(() => resultAlias.Phone)
						.Select(Projections.Property(() => operatorAlias.Id)).WithAlias(() => resultAlias.OperatorId)
					)
					.TransformUsing(Transformers.AliasToBean<PhoneAssignment>())
					.List<PhoneAssignment>();

				return assignments;
			}
		}
	}
}
