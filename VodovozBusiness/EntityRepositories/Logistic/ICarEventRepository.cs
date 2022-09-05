using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICarEventRepository
	{
		CarEvent GetCarEventById(IUnitOfWork uow, int id); 
	}
}
