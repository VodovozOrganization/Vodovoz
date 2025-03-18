using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Phones
{
	public class OperatorPhoneService : IOperatorPhoneService
	{
		private readonly IUnitOfWork _unitOfWork;

		public OperatorPhoneService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public IEnumerable<PhoneAssignment> GetPhoneAssignments()
		{
			return (from innerPhone in _unitOfWork.Session.Query<InnerPhone>()
					let operatorId = (int?)
						(from pacsOperatorState in _unitOfWork.Session.Query<OperatorState>()
						 let pacsSessionIsActive =
							(from pacsSession in _unitOfWork.Session.Query<OperatorSession>()
							 where pacsSession.Ended == null
								&& pacsSession.Id == pacsOperatorState.Session.Id
							 select pacsSession.Id)
							.Any()
						 where pacsSessionIsActive
							&& pacsOperatorState.Ended == null
							&& pacsOperatorState.PhoneNumber != null
						 select pacsOperatorState.OperatorId)
						.LastOrDefault()
					select new PhoneAssignment
					{
						Phone = innerPhone.PhoneNumber,
						OperatorId = operatorId ?? 0,
					})
					.ToList();
		}

		public string GetAssignedPhone(int operatorId)
		{
			return (from pacsOperatorState in _unitOfWork.Session.Query<OperatorState>()
					let pacsSessionIsActive =
						(from pacsSession in _unitOfWork.Session.Query<OperatorSession>()
						 where pacsSession.Ended == null
						 && pacsSession.Id == pacsOperatorState.Session.Id
						 select pacsSession.Id)
						.Any()
					where pacsOperatorState.OperatorId == operatorId
						&& pacsSessionIsActive
						&& pacsOperatorState.Ended == null
						&& pacsOperatorState.PhoneNumber != null
					select pacsOperatorState.PhoneNumber)
					.LastOrDefault();
		}

		public int? GetAssignedOperator(string phone)
		{
			return (from pacsOperatorState in _unitOfWork.Session.Query<OperatorState>()
					let pacsSessionIsActive =
						(from pacsSession in _unitOfWork.Session.Query<OperatorSession>()
						 where pacsSession.Ended == null
						 && pacsSession.Id == pacsOperatorState.Session.Id
						 select pacsSession.Id)
						.Any()
					where pacsOperatorState.PhoneNumber == phone
						&& pacsSessionIsActive
						&& pacsOperatorState.Ended == null
					select pacsOperatorState.OperatorId)
					.LastOrDefault();
		}

		public bool PhoneExists(string phone)
		{
			return _unitOfWork.Session.Query<InnerPhone>()
				.Any(x => x.PhoneNumber == phone);
		}
	}
}
