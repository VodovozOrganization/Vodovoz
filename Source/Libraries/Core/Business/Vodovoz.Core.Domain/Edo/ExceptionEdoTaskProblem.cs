namespace Vodovoz.Core.Domain.Edo
{
	public class ExceptionEdoTaskProblem : EdoTaskProblem
	{
		private string _exceptionMessage;

		/// <summary>
		/// Сообщение исключения
		/// </summary>
		public virtual string ExceptionMessage
		{
			get => _exceptionMessage;
			set => SetField(ref _exceptionMessage, value);
		}
	}
}
