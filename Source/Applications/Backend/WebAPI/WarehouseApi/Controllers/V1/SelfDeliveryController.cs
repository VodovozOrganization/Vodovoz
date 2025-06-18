using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using VodovozBusiness.Employees;
using WarehouseApi.Contracts.Requests.V1;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Library.Extensions;
using WarehouseApi.Library.Services;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с самовывозами
	/// </summary>
	[OnlyOneSession]
	[Route("api/[controller]")]
	public class SelfDeliveryController : VersionedController
	{
		private const string _rolesToAccess = nameof(ApplicationUserRole.WarehousePicker);

		private readonly UserManager<IdentityUser> _userManager;
		private readonly IExternalApplicationUserService _externalApplicationUserService;
		private readonly ISelfDeliveryService _selfDeliveryService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="externalApplicationUserService"></param>
		/// <param name="selfDeliveryService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelfDeliveryController(
			ILogger<SelfDeliveryController> logger,
			UserManager<IdentityUser> userManager,
			IExternalApplicationUserService externalApplicationUserService,
			ISelfDeliveryService selfDeliveryService)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_externalApplicationUserService = externalApplicationUserService
				?? throw new ArgumentNullException(nameof(externalApplicationUserService));
			_selfDeliveryService = selfDeliveryService
				?? throw new ArgumentNullException(nameof(selfDeliveryService));
		}

		/// <summary>
		/// Получение информацию о заказе самовывоза по идентификатору документа отпуска самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="selfDeliveryDocumentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(GetSelfDeliveryResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetAsync(
			int? orderId,
			int? selfDeliveryDocumentId,
			CancellationToken cancellationToken)
			=> await GetDocumentByOrderIdOrSelfDeliveryDocumentId(orderId, selfDeliveryDocumentId, cancellationToken)
				.MatchAsync<SelfDeliveryDocument, IActionResult>(
					selfDeliveryDocument =>
					{
						var nomenclatures = selfDeliveryDocument.Order.OrderItems
							.Select(x => x.Nomenclature)
							.ToArray()
							.AsEnumerable();

						return Ok(
							new GetSelfDeliveryResponse
							{
								SelfDeliveryDocumentId = selfDeliveryDocument.Id,
								Order = selfDeliveryDocument.Order.ToApiDtoV1(nomenclatures)
							});
					},
					errors => Problem(
						string.Join(", ", errors.Select(e => e.Message)),
						statusCode: StatusCodes.Status400BadRequest));

		/// <summary>
		/// Создание документа отпуска самовывоза
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut]
		[ProducesResponseType(StatusCodes.Status201Created)]
		public async Task<IActionResult> Put(
			[FromServices] IUnitOfWork unitOfWork,
			PutSelfDeliveryRequest request,
			CancellationToken cancellationToken)
			=> await GetUserAsync(User)
				.BindAsync(user =>
					_externalApplicationUserService.GetExternalUserEmployee(
						user.UserName,
						ExternalApplicationType.WarehouseApp,
						cancellationToken))
				.BindAsync(employee =>
					_selfDeliveryService.CreateDocument(employee, request.OrderId, request.WarehouseId, cancellationToken))
				.BindAsync(selfDeliveryDocument =>
					_selfDeliveryService.AddCodes(selfDeliveryDocument, request.CodesToAdd, cancellationToken))
				.BindAsync(selfDeliveryDocument => EndLoadIfNeededAsync(request.EndLoad, selfDeliveryDocument, cancellationToken))
				.BindAsync(async selfDeliveryDocument =>
				{
					await unitOfWork.SaveAsync(selfDeliveryDocument, cancellationToken: cancellationToken);
					await unitOfWork.CommitAsync(cancellationToken);
					return Result.Success(selfDeliveryDocument);
				})
				.MatchAsync(
					selfDeliveryDocument => Created(
						Url.Action(
							nameof(GetAsync),
							controller: nameof(SelfDeliveryController),
							new { SelfDeliveryDocumentId = selfDeliveryDocument.Id })
						, selfDeliveryDocument),
					errors => Problem(
						string.Join(", ", errors.Select(x => x.Message)),
						statusCode: StatusCodes.Status400BadRequest));


		/// <summary>
		/// Изменение документа отпуска самовывоза
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPatch]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Patch(
			[FromServices] IUnitOfWork unitOfWork,
			PatchSelfDeliveryRequest request,
			CancellationToken cancellationToken)
			=> await _selfDeliveryService
				.GetSelfDeliveryDocumentById(request.SelfDeliveryDocumentId, cancellationToken)
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.RemoveCodes(selfDeliveryDocument, request.CodesToDelete, cancellationToken))
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.ChangeCodes(selfDeliveryDocument, request.CodesToChange, cancellationToken))
				.BindAsync(selfDeliveryDocument => _selfDeliveryService.AddCodes(selfDeliveryDocument, request.CodesToAdd, cancellationToken))
				.BindAsync(selfDeliveryDocument => EndLoadIfNeededAsync(request.EndLoad, selfDeliveryDocument, cancellationToken))
				.BindAsync(async selfDeliveryDocument =>
				{
					await unitOfWork.SaveAsync(selfDeliveryDocument, cancellationToken: cancellationToken);
					await unitOfWork.CommitAsync(cancellationToken);
					return Result.Success(selfDeliveryDocument);
				})
				.MatchAsync<SelfDeliveryDocument, IActionResult>(
					selfDeliveryDocument => NoContent(),
					errors => Problem(
						string.Join(", ", errors.Select(x => x.Message)),
						statusCode: StatusCodes.Status400BadRequest));


		private async Task<Result<SelfDeliveryDocument>> GetDocumentByOrderIdOrSelfDeliveryDocumentId(int? orderId, int? selfDeliveryDocumentId, CancellationToken cancellationToken)
		{
			if(selfDeliveryDocumentId is null && orderId is null)
			{
				return Result.Failure<SelfDeliveryDocument>(new Error("Temp.Error", "Не указан идентификатор документа самовывоза или идентификатор заказа самовывоза"));
			}

			if(selfDeliveryDocumentId != null)
			{
				return await _selfDeliveryService.GetSelfDeliveryDocumentById(selfDeliveryDocumentId.Value, cancellationToken);
			}
			else
			{
				return await _selfDeliveryService.GetSelfDeliveryDocumentByOrderId(orderId.Value, cancellationToken);
			}
		}

		private async Task<Result<SelfDeliveryDocument>> EndLoadIfNeededAsync(bool endLoadNeeded, SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
		{
			if(endLoadNeeded)
			{
				await _selfDeliveryService.EndLoad(selfDeliveryDocument, cancellationToken);
			}

			return selfDeliveryDocument;
		}

		private async Task<Result<IdentityUser>> GetUserAsync(ClaimsPrincipal userClaims)
		{
			if(await _userManager.GetUserAsync(userClaims) is IdentityUser identityUser)
			{
				return identityUser;
			}
			else
			{
				return Result.Failure<IdentityUser>(new Error("Temp.Error", "Не удалось получить пользователя"));
			}
		}
	}
}
