using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Edo
{
	public interface IUpdEnventPositionsTask
	{
		int Id { get; set; }
		IList<EdoUpdInventPosition> UpdInventPositions { get; set; }
	}
}
