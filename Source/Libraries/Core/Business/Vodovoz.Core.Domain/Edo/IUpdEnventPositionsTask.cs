using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Задача со строками УПД документа
	/// </summary>
	public interface IUpdEnventPositionsTask
	{
		int Id { get; set; }
		IList<EdoUpdInventPosition> UpdInventPositions { get; set; }
	}
}
