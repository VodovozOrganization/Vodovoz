using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum PersonType
	{
		[Display(Name = "Физическое лицо")]
		natural,
		[Display(Name = "Юридическое лицо")]
		legal
	}
}
