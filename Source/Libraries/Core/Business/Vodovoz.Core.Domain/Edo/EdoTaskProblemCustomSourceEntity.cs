namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemCustomSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemDescriptionSourceType Type => 
			EdoTaskProblemDescriptionSourceType.Custom;

		public virtual string Message { get; set; }
	}
}
