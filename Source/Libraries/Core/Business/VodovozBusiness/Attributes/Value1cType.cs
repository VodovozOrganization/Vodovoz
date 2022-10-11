using System;

namespace Vodovoz.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1cType : Attribute
	{
		public string Value { get; }

		public Value1cType(string value)
		{
			this.Value = value;
		}
	}
}
