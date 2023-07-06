using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Controllers
{
	public interface INomenclatureOnlineParametersController
	{
		IDictionary<int, NomenclatureOnlineParameters> GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, NomenclatureOnlineParameterType parameterType);
	}
}
