using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	/// <summary>
	/// Неизвестная ошибка
	/// </summary>
	public class UnknownException : EdoTaskProblemExceptionSource
	{
		public override string Name => "UnknownException";
		public override string Description => "Произошла неизвестная ошибка";
		public override string Recommendation => "Обратитесь в РПО";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		
		public static UnknownException Create() => new UnknownException();
	}
}
