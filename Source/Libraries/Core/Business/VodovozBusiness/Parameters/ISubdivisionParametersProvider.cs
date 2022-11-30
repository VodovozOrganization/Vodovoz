namespace Vodovoz.Parameters
{
	public interface ISubdivisionParametersProvider
	{
		int GetDevelopersSubdivisionId { get; }
		int GetOkkId();
		int GetSubdivisionIdForRouteListAccept();
		int GetParentVodovozSubdivisionId();
		int GetSalesSubdivisionId();
	}
}
