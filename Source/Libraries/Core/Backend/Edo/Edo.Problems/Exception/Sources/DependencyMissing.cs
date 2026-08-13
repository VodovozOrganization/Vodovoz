using Autofac.Core;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.Sources
{
	/// <summary>
	/// Ошибка при отсутствии необходимой зависимости
	/// </summary>
	public class DependencyMissing : EdoTaskProblemExceptionSource
	{
		public override string Name => typeof(DependencyResolutionException).ToString();
		public override string Description => "Нарушена работа важного сервиса. Не хватает обязательной зависимости";
		public override string Recommendation => "Обратитесь в РПО";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
