namespace Vodovoz.Presentation.WebApi.Caching
{
	public class ResponseInfo<T>
		where T : class
	{
		public T Response { get; set; }
		public int StatusCode { get; set; }
	}
}
