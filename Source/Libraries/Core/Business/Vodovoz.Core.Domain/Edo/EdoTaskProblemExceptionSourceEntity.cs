namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemExceptionSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemDescriptionSourceType Type =>
			EdoTaskProblemDescriptionSourceType.Exception;
	}
}
