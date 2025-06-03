using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Domain.Goods.Recomendations
{
	public interface IRecomendationService
	{
		IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
		IEnumerable<RecomendationItem> GetRecomendationItemsForRobot(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
		IEnumerable<RecomendationItem> GetRecomendationItemsForIpz(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
	}
}
