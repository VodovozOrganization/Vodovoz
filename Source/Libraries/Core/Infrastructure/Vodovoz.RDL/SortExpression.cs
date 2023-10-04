using System;
using Vodovoz.RDL.Elements;

namespace Vodovoz.RDL
{
	public class SortExpression
	{
		private SortByTypeDirection _sortDirection;
		private string _sortExpressionString;

		public SortExpression(string sortExpressionString, SortByTypeDirection sortDirection)
		{
			if(string.IsNullOrWhiteSpace(sortExpressionString))
			{
				throw new ArgumentException($"'{nameof(sortExpressionString)}' cannot be null or whitespace.", nameof(sortExpressionString));
			}

			_sortExpressionString = sortExpressionString;
			_sortDirection = sortDirection;
		}

		public SortByTypeDirection SortDirection => _sortDirection;

		public string SortExpressionString => _sortExpressionString;

	}
}
