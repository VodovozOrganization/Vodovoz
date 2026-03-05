namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="Success"></param>
	/// <param name="StatusCode"></param>
	/// <param name="Title"></param>
	/// <param name="Detail"></param>
	public record CancelOrderResult(bool Success, int StatusCode, string Title, string Detail);
}
