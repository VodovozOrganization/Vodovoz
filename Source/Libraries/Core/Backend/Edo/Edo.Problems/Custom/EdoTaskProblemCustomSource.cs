using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom
{
	public abstract class EdoTaskProblemCustomSource : EdoTaskProblemCustomSourceEntity
	{
		public abstract override string Name { get; }
		public abstract override string Message { get; }
		public abstract override string Description { get; }
		public abstract override string Recommendation { get; }
		public abstract override EdoProblemImportance Importance { get; }

		public virtual string GetTemplatedMessage(EdoTask edoTask)
		{
			return Message;
		}
	}
}
