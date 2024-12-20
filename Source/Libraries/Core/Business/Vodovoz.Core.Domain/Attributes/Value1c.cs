using System;

namespace Vodovoz.Core.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1c : Attribute
	{
		public string Value { get; }

		public Value1c(string value)
		{
			Value = value;
		}
	}
}

