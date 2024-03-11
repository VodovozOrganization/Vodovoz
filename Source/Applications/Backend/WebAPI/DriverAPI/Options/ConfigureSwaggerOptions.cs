using DriverApi.Contracts;
using FastPaymentsApi.Contracts;
using LogisticsEventsApi.Contracts;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using Vodovoz.Presentation.WebApi.SwaggerGen;

namespace DriverAPI.Options
{
	internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
	{
		private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

		public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			_apiVersionDescriptionProvider = apiVersionDescriptionProvider
				?? throw new ArgumentNullException(nameof(apiVersionDescriptionProvider));
		}

		public void Configure(SwaggerGenOptions options)
		{
			options.OperationFilter<ProblemDetailsOperationFilter>();

			options.MapType<TimeSpan>(() => new OpenApiSchema
			{
				Type = "string",
				Example = new OpenApiString("00:00:00")
			});

			options.MapType<TimeSpan?>(() => new OpenApiSchema
			{
				Type = "string",
				Example = new OpenApiString("00:00:00")
			});

			options.DocumentFilter<EnumTypesDocumentFilter>();

			foreach (var desvription in _apiVersionDescriptionProvider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(desvription.GroupName, CreateVersionInfo(desvription));

				AddCommentsForAssembly<ConfigureSwaggerOptions>(options);
				AddCommentsForAssembly<IDriverApiContractsAssemblyMarker>(options);
				AddCommentsForAssembly<IWebApiAuthenticationContractsAssemblyMarker>(options);
				AddCommentsForAssembly<IFastPaymentsApiContractsAssemblyMarker>(options);
				AddCommentsForAssembly<ILogisticsEventsApiAssemblyMarker>(options);
			}
		}

		private void AddCommentsForAssembly<TMarker>(SwaggerGenOptions options)
			where TMarker : class
		{
			var libraryXmlFilename = $"{typeof(TMarker).Assembly.GetName().Name}.xml";
			var libraryXmlPath = Path.Combine(AppContext.BaseDirectory, libraryXmlFilename);

			options.IncludeXmlComments(libraryXmlPath);
			options.SchemaFilter<EnumTypesSchemaFilter>(libraryXmlPath);
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
