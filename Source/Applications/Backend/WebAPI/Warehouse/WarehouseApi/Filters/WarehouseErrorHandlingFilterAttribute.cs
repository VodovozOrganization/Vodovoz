using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WarehouseApi.Contracts.V1.Responses;
using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Filters
{
	public class WarehouseErrorHandlingFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(ExceptionContext context)
		{
			var logger = context.HttpContext.RequestServices
				.GetService<ILogger<WarehouseErrorHandlingFilterAttribute>>();

			var exception = context.Exception;

			logger.LogCritical(exception,
				"Произошла критическая ошибка: {ExceptionMessage}",
				exception.Message);

			var errorResponse = new WarehouseApiResponseBase
			{
				Result = OperationResultEnumDto.Error,
				Error = "При обработке запроса произошла непредвиденная ошибка. Обратитесь в техподдержку.",
			};

			context.Result = new ObjectResult(errorResponse)
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};

			context.ExceptionHandled = true;
		}
	}
}
