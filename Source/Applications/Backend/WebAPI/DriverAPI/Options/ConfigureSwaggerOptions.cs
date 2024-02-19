using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

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
			foreach (var desvription in _apiVersionDescriptionProvider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(desvription.GroupName, CreateVersionInfo(desvription));

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

				// using System.Reflection;
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

				options.IncludeXmlComments(xmlPath);
				options.SchemaFilter<EnumTypesSchemaFilter>(xmlPath);

				var libraryXmlFilename = $"{typeof(Library.DependencyInjection).Assembly.GetName().Name}.xml";
				var libraryXmlPath = Path.Combine(AppContext.BaseDirectory, libraryXmlFilename);

				options.IncludeXmlComments(libraryXmlPath);
				options.SchemaFilter<EnumTypesSchemaFilter>(libraryXmlPath);

				options.DocumentFilter<EnumTypesDocumentFilter>();
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
