using QS.DomainModel.UoW;
using System;
using System.Linq;

namespace Email.Infrastructure.Repositories
{
	public class DataBaseRepositiry : IDatabaseRepository
	{
		public int GetCurrentDatabaseId(IUnitOfWork unitOfWork)
		{
			var instanceId = Convert.ToInt32(
				unitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			return instanceId;
		}
	}
}
