using System;

namespace Vodovoz.Core.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Code1c : Attribute
	{
		public string Code;
		public Code1c(string code)
		{
			Code = code;
		}
	}
}

