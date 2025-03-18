namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemValidatorSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemType Type =>
			EdoTaskProblemType.Validation;

		public virtual string Message { get; set; }
	}
}
