using System.ComponentModel;
using Vodovoz.Core.Domain.Attributes;

namespace Vodovoz.Core.Domain.Cash
{
	public enum Vat1cType
	{
		/// <summary>
		/// БезНДС
		/// </summary>
		[Value1cType("БезНДС")]
		No,
		
		/// <summary>
		/// Общая
		/// </summary>
		[Value1cType("Общая")]
		Common,
		
		/// <summary>
		/// Пониженная
		/// </summary>
		[Value1cType("Пониженная")]
		Reduced,
		
		/// <summary>
		/// ИП
		/// </summary>
		[Value1cType("ИП")]
		IndividualEntrepreneur
		
		
	}
}
