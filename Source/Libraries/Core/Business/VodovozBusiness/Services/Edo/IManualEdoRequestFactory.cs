using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.Edo
{
	public interface IManualEdoRequestFactory
	{
		ManualEdoRequest Create(IUnitOfWork uow, OrderEntity order);
		ManualEdoRequest Create(IUnitOfWork uow, OrderEntity order, IEnumerable<TrueMarkProductCode> productCodes);
	}
}
