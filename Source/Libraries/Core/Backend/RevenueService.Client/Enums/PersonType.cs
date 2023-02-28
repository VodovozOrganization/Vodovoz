using System.ComponentModel.DataAnnotations;

namespace RevenueService.Client.Enums
{
	public enum PersonType
	{
		[Display(Name="Юр.лицо")]
		Legal,
		[Display(Name = "ИП")]
		Individual
	}
}
