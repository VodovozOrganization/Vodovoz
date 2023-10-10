namespace Vodovoz.RDL.Elements
{
	public partial class SortBy
	{
		public void AddSortExpression(SortExpression sortExpression)
		{
			ItemsList.Add(sortExpression?.SortExpressionString);
			ItemsList.Add(sortExpression?.SortDirection);
		}
	}
}
