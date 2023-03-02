using System.ComponentModel.DataAnnotations;

namespace RevenueService.Client.Enums
{
	public enum BranchType
	{
		[Display(Name="Головная")]
		Main,
		[Display(Name = "Филиал")]
		Branch
	}
}
