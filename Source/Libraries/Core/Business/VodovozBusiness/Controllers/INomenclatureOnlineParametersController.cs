using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public interface INomenclatureOnlineParametersController
	{
		NomenclatureOnlineParametersData GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, NomenclatureOnlineParameterType parameterType);
		IList<NomenclatureCharacteristicsDto> GetNomenclaturesForSend(IUnitOfWork uow, NomenclatureOnlineParameterType parameterType);
	}
}
