using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Organizations
{
	public interface IRoboatsRepository
	{
		IEnumerable<IRoboatsEntity> GetExportedEntities(RoboatsEntityType roboatsEntityType);
		IEnumerable<Nomenclature> GetWaterTypesForRoboats();
	}
}
