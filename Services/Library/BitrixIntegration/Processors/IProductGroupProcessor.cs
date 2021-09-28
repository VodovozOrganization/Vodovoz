using Bitrix.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace BitrixIntegration.Processors
{
	public interface IProductGroupProcessor
	{
		ProductGroup ProcessProductGroup(IUnitOfWork uow, DealProductItem productFromDeal);
	}
}
