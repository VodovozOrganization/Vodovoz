using System;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.CachingRepositories.Employees
{
	public interface IEmployeeInMemoryNameWithInitialsCacheRepository : IDomainEntityNodeInMemoryCacheRepository<Employee>, IDisposable
	{
	}
}
