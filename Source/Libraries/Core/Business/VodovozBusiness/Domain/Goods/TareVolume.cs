using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum TareVolume
	{
		[Display(Name = "19 л.")]
		Vol19L = 19000,
		[Display(Name = "6 л.")]
		Vol6L = 6000,
		[Display(Name = "1,5 л.")]
		Vol1500ml = 1500,
		[Display(Name = "0,6 л.")]
		Vol600ml = 600,
		[Display(Name = "0,5 л.")]
		Vol500ml = 500
	}
}
