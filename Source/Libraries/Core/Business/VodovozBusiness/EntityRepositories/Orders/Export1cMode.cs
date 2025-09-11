using System.ComponentModel.DataAnnotations;

namespace Vodovoz.EntityRepositories.Orders
{
	/// <summary>
	/// Тип эскпорта 1С
	/// </summary>
	public enum Export1cMode
	{
		BuhgalteriaOOO,
		IPForTinkoff,
		BuhgalteriaOOONew,
		/// <summary>
		///Комплексная автоматизация
		/// </summary>
		[Display(Name = "Безнал-КА")]
		ComplexAutomation,
		/// <summary>
		///Розничная продажа
		/// </summary>
		[Display(Name = "Розница")]
		Retail
	}
}
