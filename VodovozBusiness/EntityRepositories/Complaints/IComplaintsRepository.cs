using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using static Vodovoz.EntityRepositories.Complaints.ComplaintsRepository;

namespace Vodovoz.EntityRepositories.Complaints
{
	public interface IComplaintsRepository
	{
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		int GetUnclosedComplaintsCount(IUnitOfWork uow, bool? withOverdue = null, DateTime? start = null, DateTime? end = null);
		IList<object[]> GetComplaintsResults(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
	}
}