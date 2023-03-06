using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		int GetCodeErrorsOrdersCount(IUnitOfWork uow);
	}
}