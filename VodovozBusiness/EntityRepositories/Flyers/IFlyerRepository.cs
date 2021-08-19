using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.Flyers
{
	public interface IFlyerRepository
	{
		IList<int> GetAllFlyersNomenclaturesIds(IUnitOfWork uow);
		IList<Flyer> GetAllActiveFlyers(IUnitOfWork uow);
		IList<int> GetAllActiveFlyersNomenclaturesIds(IUnitOfWork uow);
		bool ExistsFlyerForNomenclatureId(IUnitOfWork uow, int nomenclatureId);
	}
}