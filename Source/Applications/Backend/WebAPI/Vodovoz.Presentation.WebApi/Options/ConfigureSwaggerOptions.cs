using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using Vodovoz.Presentation.WebApi.SwaggerGen;

namespace Vodovoz.Presentation.WebApi.Options
{
	internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
	{
		private readonly ILogger<ConfigureSwaggerOptions> _logger;
		private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

		public ConfigureSwaggerOptions(ILogger<ConfigureSwaggerOptions> logger, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
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

			foreach(var description in _apiVersionDescriptionProvider
				.ApiVersionDescriptions)
			{
				options.SwaggerDoc(description.GroupName, CreateVersionInfo(description, Assembly.GetEntryAssembly().GetName().Name));
			}

			AddCommentsForAssembly(options, Assembly.GetEntryAssembly().GetName().Name);

			var referencedContractsAssembliesByEntryAssembly = Assembly.GetEntryAssembly().GetReferencedAssemblies().Where(ra => ra.Name.EndsWith(".Contracts"));

			foreach(var assemblyName in referencedContractsAssembliesByEntryAssembly)
			{
				AddCommentsForAssembly(options, assemblyName.Name);
			}

			AddCommentsForAssembly(options, typeof(IWebApiAuthenticationContractsAssemblyMarker).Assembly.GetName().Name);
		}

		private void AddCommentsForAssembly(SwaggerGenOptions options, string assemblyName)
		{
			var libraryXmlFilename = $"{assemblyName}.xml";
			var libraryXmlPath = Path.Combine(AppContext.BaseDirectory, libraryXmlFilename);

			try
			{
				options.IncludeXmlComments(libraryXmlPath);
				options.SchemaFilter<EnumTypesSchemaFilter>(libraryXmlPath);
			}
			catch(FileNotFoundException fileNotFoundException)
			{
				_logger.LogError(fileNotFoundException, "Documentation not found for referenced Assembly: {AssemblyName}", assemblyName);
			}
		}

		private OpenApiInfo CreateVersionInfo(ApiVersionDescription description, string apiName)
		{
			var info = new OpenApiInfo
			{
				Title = apiName,
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
