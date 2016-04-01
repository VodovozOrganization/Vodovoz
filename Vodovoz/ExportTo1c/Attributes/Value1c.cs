using System;

namespace Vodovoz
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1c:Attribute
	{
		public string Value;
		public Value1c(string value)
		{
			this.Value = value;
		}
	}
}

