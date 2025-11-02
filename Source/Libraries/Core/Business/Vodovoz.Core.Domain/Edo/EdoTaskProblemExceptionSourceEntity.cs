namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemExceptionSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemType Type =>
			EdoTaskProblemType.Exception;
	}
}
