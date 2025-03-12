namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemCustomSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemType Type =>
			EdoTaskProblemType.Custom;

		public virtual string Message { get; set; }
	}
}
