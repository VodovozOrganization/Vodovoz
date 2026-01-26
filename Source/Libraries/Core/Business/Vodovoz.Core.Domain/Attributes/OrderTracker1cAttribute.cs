using System;

namespace Vodovoz.Core.Domain.Attributes
{
	/// <summary>
	/// Аттрибут для отслеживания изменений для экспорта в 1С
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class OrderTracker1cAttribute : Attribute
	{
	}
}
