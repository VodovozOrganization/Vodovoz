using System;

namespace Vodovoz.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1c : Attribute
	{
		public string Value { get; }

		public Value1c(string value)
		{
			this.Value = value;
		}
	}
}

