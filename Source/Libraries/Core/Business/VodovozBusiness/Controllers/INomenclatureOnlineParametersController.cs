using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public interface INomenclatureOnlineParametersController
	{
		NomenclatureOnlineParametersData GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
	}
}
