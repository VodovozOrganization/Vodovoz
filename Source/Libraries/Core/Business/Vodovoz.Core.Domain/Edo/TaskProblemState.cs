using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum TaskProblemState
	{
		[Display(Name = "Не решена")]
		Active,

		[Display(Name = "Решена")]
		Solved
	}
}
