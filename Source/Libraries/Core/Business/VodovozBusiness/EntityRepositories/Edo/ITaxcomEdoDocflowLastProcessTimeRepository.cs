using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.EntityRepositories.Edo
{
	public interface ITaxcomEdoDocflowLastProcessTimeRepository
	{
		TaxcomEdoDocflowLastProcessTime GetTaxcomEdoDocflowLastProcessTime(IUnitOfWork uow, string edoAccount);
	}
}
