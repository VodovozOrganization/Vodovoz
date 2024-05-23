using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum PersonType
	{
		[Display(Name = "Физическое лицо")]
		natural,
		[Display(Name = "Юридическое лицо")]
		legal
	}
}
