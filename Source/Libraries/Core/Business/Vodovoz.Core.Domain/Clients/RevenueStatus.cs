using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Статус контрагента в налоговой
	/// </summary>
	
	public enum RevenueStatus
	{
		/// <summary>
		/// Действующий
		/// </summary>
		[Display(Name = "Действующий")]
		Active,
		
		/// <summary>
		/// Ликвидируется
		/// </summary>
		[Display(Name = "Ликвидируется")]
		Liquidating,

		/// <summary>
		/// Ликвидирован
		/// </summary>
		[Display(Name = "Ликвидирован")]
		Liquidated,
		
		/// <summary>
		/// В процессе присоединения к другому юрлицу, с последующей ликвидацией
		/// </summary>
		[Display(Name = "В процессе присоединения к другому юрлицу, с последующей ликвидацией")]
		Reorganizing,
		
		/// <summary>
		/// Банкротство
		/// </summary>
		[Display(Name = "Банкротство")]
		Bankrupt
	}
}
