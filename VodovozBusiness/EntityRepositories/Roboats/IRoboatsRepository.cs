using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.EntityRepositories.Roboats
{
	public interface IRoboatsRepository
	{
		IEnumerable<IRoboatsEntity> GetExportedEntities(RoboatsEntityType roboatsEntityType);
	}
}
