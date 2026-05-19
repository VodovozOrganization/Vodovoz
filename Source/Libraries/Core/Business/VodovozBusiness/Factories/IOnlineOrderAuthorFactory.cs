using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Factories
{
	public interface IOnlineOrderAuthorFactory
	{
		Employee Create(IUnitOfWork uow, Source source);
		Task<Employee> CreateAsync(IUnitOfWork uow, Source source, CancellationToken cancellationToken);
	}
}
