using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception
{

	public abstract class EdoTaskProblemExceptionSource : EdoTaskProblemExceptionSourceEntity
	{
		public abstract override string Name { get; }
		public abstract override string Description { get; }
		public abstract override string Recommendation { get; }
		public abstract override EdoProblemImportance Importance { get; }

		public virtual string GetTemplatedMessage(EdoTask edoTask)
		{
			return Name;
		}
	}
}
