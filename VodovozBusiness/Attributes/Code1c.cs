using System;

namespace Vodovoz
{
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class Code1c : Attribute
	{
		public string Code;
		public Code1c(string code)
		{
			this.Code = code;
		}
	}
}

