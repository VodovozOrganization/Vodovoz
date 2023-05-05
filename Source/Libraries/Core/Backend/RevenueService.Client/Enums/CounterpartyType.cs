using System.ComponentModel.DataAnnotations;

namespace RevenueService.Client.Enums
{
	public enum CounterpartyType
	{
		[Display(Name="Юр.лицо")]
		Legal,
		[Display(Name = "ИП")]
		Individual
	}
}
