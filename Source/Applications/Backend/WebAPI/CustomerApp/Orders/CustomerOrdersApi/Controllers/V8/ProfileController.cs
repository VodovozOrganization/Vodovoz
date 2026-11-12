using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V8
{
	/// <summary>
	/// Профиль клиента
	/// </summary>
	[ApiVersion("8.0")]
	public sealed class ProfileController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;

		public ProfileController(
			ILogger<ProfileController> logger,
			ICustomerOrdersService customerOrdersService) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
		}

		/// <summary>
		/// Получение профиля клиента с бонусным счётом
		/// </summary>
		/// <param name="getProfileInfoDto">Данные для получения профиля</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Профиль клиента <see cref="ProfileInfoDto"/></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProfileInfoDto))]
		public async Task<IActionResult> GetProfileInfo(
			[FromBody] GetProfileInfoDto getProfileInfoDto,
			CancellationToken cancellationToken)
		{
			try
			{
				if(!_customerOrdersService.ValidateProfileInfoSignature(getProfileInfoDto, out var generatedSignature))
				{
					return InvalidSignature(getProfileInfoDto.Signature, generatedSignature);
				}

				var result = await _customerOrdersService
					.GetProfileInfo(getProfileInfoDto, cancellationToken)
					.ConfigureAwait(false);

				if(result.HttpCode != StatusCodes.Status200OK)
				{
					return BadRequest(new ProblemDetails
					{
						Status = result.HttpCode,
						Detail = result.Message
					});
				}

				return Ok(result.Profile);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка получения профиля клиента {ErpCounterpartyId}",
					getProfileInfoDto?.ErpCounterpartyId);

				return Problem();
			}
		}
	}
}
