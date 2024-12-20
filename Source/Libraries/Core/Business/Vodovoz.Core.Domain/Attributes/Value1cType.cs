using System;

namespace Vodovoz.Core.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Value1cType : Attribute
	{
		public string Value { get; }

		public Value1cType(string value)
		{
			Value = value;
		}
	}
}
