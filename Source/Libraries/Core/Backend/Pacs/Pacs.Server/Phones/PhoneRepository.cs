using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;

namespace Pacs.Server.Phones
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
				var assignments = uow.Session
					.CreateSQLQuery(
$@"SELECT 
	ip.phone_number as {nameof(PhoneAssignment.Phone)}, 
	phone_assignments.operator_id as {nameof(PhoneAssignment.OperatorId)}
FROM inner_phones ip
LEFT JOIN (
	SELECT 
		pos.operator_id, 
		pos.phone_number
	FROM pacs_operator_states pos
		LEFT JOIN pacs_sessions ps ON ps.id = pos.session_id
	WHERE 
		ps.ended IS NULL
		AND pos.ended IS NULL
		AND pos.phone_number IS NOT NULL
	) phone_assignments ON 
	phone_assignments.phone_number = ip.phone_number
GROUP BY ip.phone_number
;")
					.AddScalar(nameof(PhoneAssignment.Phone), NHibernateUtil.String)
					.AddScalar(nameof(PhoneAssignment.OperatorId), NHibernateUtil.Int32)
					.SetResultTransformer(Transformers.AliasToBean<PhoneAssignment>())
					.List<PhoneAssignment>();

				return assignments;
			}
		}
	}
}
