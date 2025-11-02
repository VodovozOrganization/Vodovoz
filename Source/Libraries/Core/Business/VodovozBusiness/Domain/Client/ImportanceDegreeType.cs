using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ImportanceDegreeType
	{
		[Display(Name = "Нет")]
		Nope,
		[Display(Name = "Важно")]
		Important
	}
}
