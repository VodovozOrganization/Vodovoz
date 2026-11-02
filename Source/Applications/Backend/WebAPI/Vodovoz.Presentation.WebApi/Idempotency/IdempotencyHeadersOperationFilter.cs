using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Reflection;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	/// <summary>
	/// Добавляет в описание методов с <see cref="IdempotentAttribute"/> необязательные заголовки идемпотентности
	/// </summary>
	internal sealed class IdempotencyHeadersOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if(context.MethodInfo?.GetCustomAttribute<IdempotentAttribute>() is null)
			{
				return;
			}

			if(operation.Parameters is null)
			{
				operation.Parameters = new List<OpenApiParameter>();
			}

			operation.Parameters.Add(new OpenApiParameter
			{
				Name = IdempotencyRequestHeadersNames.IdempotencyKey,
				In = ParameterLocation.Header,
				Required = false,
				Description = "Ключ операции; повтор с тем же ключом получает сохранённый ответ",
				Schema = new OpenApiSchema
				{
					Type = "string",
					Format = "uuid"
				}
			});

			operation.Parameters.Add(new OpenApiParameter
			{
				Name = IdempotencyRequestHeadersNames.ActionTimeUtc,
				In = ParameterLocation.Header,
				Required = false,
				Description = "Время действия в МП, UTC, формат yyyy-MM-ddTHH:mm:ssZ, например 2026-08-12T13:34:08Z",
				Schema = new OpenApiSchema
				{
					Type = "string",
					Format = "date-time"
				}
			});
		}
	}
}
