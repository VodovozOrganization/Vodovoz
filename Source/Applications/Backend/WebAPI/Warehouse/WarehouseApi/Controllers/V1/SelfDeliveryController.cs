using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using VodovozBusiness.Employees;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.Requests.V1;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Contracts.V1.Responses;
using WarehouseApi.Library.Extensions;
using WarehouseApi.Library.Services;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с самовывозами
	/// </summary>
	[Authorize(Roles = _rolesToAccess)]
	[OnlyOneSession]
	[Route("api/[controller]")]
	public class SelfDeliveryController : VersionedController
	{
		private const string _rolesToAccess = nameof(ApplicationUserRole.WarehousePicker);

		private readonly UserManager<IdentityUser> _userManager;
		private readonly IExternalApplicationUserService _externalApplicationUserService;
		private readonly ISelfDeliveryService _selfDeliveryService;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="externalApplicationUserService"></param>
		/// <param name="selfDeliveryService"></param>
		/// <param name="trueMarkWaterCodeService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelfDeliveryController(
			ILogger<SelfDeliveryController> logger,
			UserManager<IdentityUser> userManager,
			IExternalApplicationUserService externalApplicationUserService,
			ISelfDeliveryService selfDeliveryService,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_externalApplicationUserService = externalApplicationUserService
				?? throw new ArgumentNullException(nameof(externalApplicationUserService));
			_selfDeliveryService = selfDeliveryService
				?? throw new ArgumentNullException(nameof(selfDeliveryService));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

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
				.BindAsync(employee =>
					_selfDeliveryService.SetTareToReturn(employee, request.TareToReturn))
				.BindAsync(selfDeliveryDocument => EndLoad(selfDeliveryDocument, cancellationToken))
				.BindAsync(async selfDeliveryDocument =>
				{
					await unitOfWork.SaveAsync(selfDeliveryDocument, cancellationToken: cancellationToken);
					await unitOfWork.CommitAsync(cancellationToken);
					return Result.Success(selfDeliveryDocument);
				})
				.BindAsync(async selfDeliveryDocument => await _selfDeliveryService.SendEdoRequest(selfDeliveryDocument, cancellationToken))
				.MatchAsync<SelfDeliveryDocument, IActionResult>(
					selfDeliveryDocument =>
					{
						var nomenclatures = selfDeliveryDocument.Order.OrderItems
							.Select(x => x.Nomenclature)
							.ToArray()
							.AsEnumerable();

						var response = new GetSelfDeliveryResponse
						{
							SelfDeliveryDocumentId = selfDeliveryDocument.Id,
							Order = selfDeliveryDocument.Order.ToApiDtoV1(nomenclatures, selfDeliveryDocument)
						};

						response.Order.Items
							.PopulateRelatedCodes(unitOfWork, _trueMarkWaterCodeService, selfDeliveryDocument.Items.SelectMany(x => x.TrueMarkProductCodes));

						response.Order.Items.ForEach(item =>
							item.Codes.ForEach((code, i) =>
								code.SequenceNumber = i));

						return Ok(response);
					},
					errors => Problem(
						string.Join(", ", errors.Select(e => e.Message)),
						statusCode: StatusCodes.Status400BadRequest));

		/// <summary>
		/// Список заказов самовывоза для склада
		/// </summary>
		/// <param name="warehouseId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("GetOrders")]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(typeof(IEnumerable<GetSelfDeliveryOrderResponse>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetOrders(
			[FromQuery] int warehouseId,
			CancellationToken cancellationToken) =>
			await _selfDeliveryService.GetSelfDeliveryOrders(warehouseId, cancellationToken)
			.MatchAsync(
				orders => Ok(orders.Select(o => o.ToGetSelfDeliveryOrderResponseDto()).ToArray()),
				errors => Problem(
					string.Join(", ", errors.Select(e => e.Message)),
					statusCode: StatusCodes.Status400BadRequest));

		private async Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
		{
			await _selfDeliveryService.EndLoad(selfDeliveryDocument, cancellationToken);

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
