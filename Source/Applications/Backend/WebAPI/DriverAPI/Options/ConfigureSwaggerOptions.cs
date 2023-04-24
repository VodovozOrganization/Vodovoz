using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace DriverAPI.Options
{
	public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
	{
		private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

		public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			_apiVersionDescriptionProvider = apiVersionDescriptionProvider
				?? throw new ArgumentNullException(nameof(apiVersionDescriptionProvider));
		}

		public void Configure(SwaggerGenOptions options)
		{
			foreach (var desvription in _apiVersionDescriptionProvider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(desvription.GroupName, CreateVersionInfo(desvription));
			}
		}

		private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
		{
			var info = new OpenApiInfo
			{
				Title = nameof(DriverAPI),
				Version = description.ApiVersion.ToString()
			};

			if(description.IsDeprecated)
			{
				info.Description = "This API version has been deprecated";
			}

			return info;
		}
	}
}
