using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Attributes;

namespace Vodovoz.Core.Domain.Cash
{
	public enum Vat1cType
	{
		/// <summary>
		/// БезНДС
		/// </summary>
		[Value1cType("БезНДС")]
		[Display(Name = "Без НДС")]
		No,
		
		/// <summary>
		/// Общая
		/// </summary>
		[Value1cType("Общая")]
		[Display(Name = "Общая")]
		Common,
		
		/// <summary>
		/// Пониженная
		/// </summary>
		[Value1cType("Пониженная")]
		[Display(Name = "Пониженная")]
		Reduced,
		
		/// <summary>
		/// ИП
		/// </summary>
		[Value1cType("ИП")]
		[Display(Name = "ИП")]
		IndividualEntrepreneur
		
		
	}
}
