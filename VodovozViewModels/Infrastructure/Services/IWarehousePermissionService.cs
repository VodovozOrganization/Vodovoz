using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouse;

namespace Vodovoz.Infrastructure.Services
{
	public interface IWarehousePermissionService
	{
		IWarehousePermissionValidator GetValidator(IUnitOfWork uow, int subdivisionId);
	}
}
