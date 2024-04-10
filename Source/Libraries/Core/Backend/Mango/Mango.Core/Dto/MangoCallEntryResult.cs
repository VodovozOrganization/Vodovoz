using System.ComponentModel.DataAnnotations;

namespace Mango.Core.Dto
{
	public enum MangoCallEntryResult
	{
		/// <summary>
		/// Звонок пропущен, разговор не состоялся
		/// </summary>
		[Display(Name = "Пропущен")]
		Missed = 0,

		/// <summary>
		/// Звонок успешен и разговор состоялся
		/// </summary>
		[Display(Name = "Успешен")]
		Sucess = 1
	}
}
