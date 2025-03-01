namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemValidatorSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemDescriptionSourceType Type =>
			EdoTaskProblemDescriptionSourceType.Validator;

		public virtual string Message { get; set; }
	}
}
