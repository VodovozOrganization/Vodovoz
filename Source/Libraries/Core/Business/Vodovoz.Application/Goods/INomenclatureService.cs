using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors;

namespace Vodovoz.Application.Goods
{
	public interface INomenclatureService
	{
		Result Archive(IUnitOfWork unitOfWork, int nomenclatureId);
		Result Archive(IUnitOfWork unitOfWork, Nomenclature nomenclature);
	}
}