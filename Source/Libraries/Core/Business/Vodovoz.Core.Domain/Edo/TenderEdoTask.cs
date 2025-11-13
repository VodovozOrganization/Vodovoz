using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class TenderEdoTask : OrderEdoTask, IUpdEnventPositionsTask
	{
		private TenderEdoTaskStage _stage;
		private IList<EdoUpdInventPosition> _updInventPositions = new List<EdoUpdInventPosition>();
		public override EdoTaskType TaskType => EdoTaskType.Tender;

		[Display(Name = "Стадия")]
		public virtual TenderEdoTaskStage Stage
		{
			get => _stage;
			set => SetField(ref _stage, value);
		}
		
		[Display(Name = "Строка УПД документа")]
		public virtual IList<EdoUpdInventPosition> UpdInventPositions
		{
			get => _updInventPositions;
			set => SetField(ref _updInventPositions, value);
		}
	}
}
