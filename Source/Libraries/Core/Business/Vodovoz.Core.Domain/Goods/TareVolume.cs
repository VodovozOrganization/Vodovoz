using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Объем тары
	/// </summary>
	public enum TareVolume
	{
		/// <summary>
		/// 19л
		/// </summary>
		[Display(Name = "19 л.")]
		Vol19L = 19000,
		/// <summary>
		/// 6л
		/// </summary>
		[Display(Name = "6 л.")]
		Vol6L = 6000,
		/// <summary>
		/// 1.5л
		/// </summary>
		[Display(Name = "1,5 л.")]
		Vol1500ml = 1500,
		/// <summary>
		/// 0.6л
		/// </summary>
		[Display(Name = "0,6 л.")]
		Vol600ml = 600,
		/// <summary>
		/// 0.5л
		/// </summary>
		[Display(Name = "0,5 л.")]
		Vol500ml = 500
	}
}
