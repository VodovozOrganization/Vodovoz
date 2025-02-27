using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	public enum OrganizationEdoType
	{
		[Display(Name = "Не участвует в ЭДО")]
		WithoutEdo,
		[Display(Name = "Производитель")]
		Producer,
		[Display(Name = "Продавец")]
		Seller
	}
}
