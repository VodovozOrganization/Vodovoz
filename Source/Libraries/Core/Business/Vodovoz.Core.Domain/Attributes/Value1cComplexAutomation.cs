using System;

namespace Vodovoz.Core.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1cComplexAutomation : Attribute
	{
		public string Value { get; }

		public Value1cComplexAutomation(string value)
		{
			Value = value;
		}
	}
}

