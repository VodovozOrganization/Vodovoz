using CustomerOrders.Abstractions.V8.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V8.Services.Validators
{
	public interface IOnlineOrderTemplateFromOnlineOrderValidator
	{
		Result Validate(IUnitOfWork uow, ICanCreateOnlineOrderTemplate canCreateTemplate);
	}
}
