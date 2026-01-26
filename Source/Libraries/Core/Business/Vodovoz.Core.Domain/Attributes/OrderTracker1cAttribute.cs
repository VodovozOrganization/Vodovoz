using System;

namespace Vodovoz.Core.Domain.Attributes
{
	/// <summary>
	/// Аттрибут для отслеживания изменений значимых для экспорта в 1С свойств
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class OrderTracker1cAttribute : Attribute
	{
	}
}
