using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderFromTemplateCreator
	{
		Task Create(IUnitOfWork uow, CancellationToken cancellation);
	}
}
