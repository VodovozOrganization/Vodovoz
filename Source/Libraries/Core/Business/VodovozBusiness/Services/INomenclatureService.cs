using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services
{
	public interface INomenclatureService
	{
		Result Archive(IUnitOfWork unitOfWork, int nomenclatureId);
		Result Archive(IUnitOfWork unitOfWork, Nomenclature nomenclature);
		void CalculateMasterCallNomenclaturePriceIfNeeded(IUnitOfWork unitOfWork, Order order);
	}
}
