using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Vodovoz.Presentation.WebApi.SwaggerGen
{
	public class ProblemDetailsOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			operation.Responses.Add(StatusCodes.Status500InternalServerError.ToString(), new OpenApiResponse
			{
				Content =
				{
					["application/problem+json"] = new OpenApiMediaType
					{
						Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
					}
				}
			});

			operation.Responses.Add(StatusCodes.Status400BadRequest.ToString(), new OpenApiResponse
			{
				Content =
				{
					["application/problem+json"] = new OpenApiMediaType
					{
						Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
					}
				}
			});

			operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse
			{
				Content =
				{
					["application/problem+json"] = new OpenApiMediaType
					{
						Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
					}
				}
			});

			operation.Responses.Add(StatusCodes.Status404NotFound.ToString(), new OpenApiResponse
			{
				Content =
				{
					["application/problem+json"] = new OpenApiMediaType
					{
						Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
					}
				}
			});
		}
	}
}
