namespace Vodovoz.Parameters
{
	public interface ISubdivisionParametersProvider
	{
		int GetOkkId();
		int GetSubdivisionIdForRouteListAccept();
		int GetParentVodovozSubdivisionId();
		int GetSalesSubdivisionId();
	}
}
