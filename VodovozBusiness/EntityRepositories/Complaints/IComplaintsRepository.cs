using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Complaints
{
	public interface IComplaintsRepository
	{
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		int GetUnclosedComplaintsCount(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		IList<object[]> GetComplaintsResults(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
	}
}