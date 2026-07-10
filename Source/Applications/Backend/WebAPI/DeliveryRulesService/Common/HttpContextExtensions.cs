using Microsoft.AspNetCore.Http;

namespace DeliveryRulesService.Common
{
	public static class HttpContextExtensions
	{
		public static string GetJsonSettingsName(this HttpContext httpContext)
		{
			var endpoint = httpContext.GetEndpoint();
			var attribute = endpoint?.Metadata.GetMetadata<JsonSettingsNameAttribute>();

			return attribute is null ? string.Empty : attribute.Name;
		}
	}
}
