using System.Collections.Generic;

namespace Vodovoz.Tools.Comparers
{
	public class StringOrNullAfterComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if(x == y)
			{
				return 0;
			}

			if(string.IsNullOrWhiteSpace(x))
			{
				return int.MaxValue;
			}

			if(string.IsNullOrWhiteSpace(y))
			{
				return int.MinValue;
			}

			return x.CompareTo(y);
		}
	}
}
