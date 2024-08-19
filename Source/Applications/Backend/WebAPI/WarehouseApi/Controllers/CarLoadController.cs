using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Requests;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Controllers
{
	[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[OnlyOneSession]
	[Route("/api/")]
	public class CarLoadController : ControllerBase
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);

		private readonly ILogger<CarLoadController> _logger;

		public CarLoadController(
			ILogger<CarLoadController> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Начало погрузки по талону погрузки погрузки
		/// </summary>
		/// <param name="documentId"></param>
		/// <returns></returns>
		[HttpPost("StartLoad")]
		public async Task<IActionResult> StartLoad([FromQuery] int documentId)
		{
			var response = new StartLoadResponse();
			response.Result = OperationResultEnumDto.Success;
			response.CarLoadDocument = new CarLoadDocumentDto
			{
				Id = documentId,
				Driver = "Super Driver",
				Car = "Super Car",
				LoadPriority = 12,
				State = LoadOperationStateEnumDto.InProgress
			};

			return Ok(response);
		}

		/// <summary>
		/// Получение информации о заказе
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("GetOrder")]
		public async Task<IActionResult> GetOrder([FromQuery] int orderId)
		{
			var response = new GetOrderResponse();
			response.Result = OperationResultEnumDto.Success;
			response.Order = new OrderDto
			{
				Id = orderId,
				CarLoadDocument = 321,
				State = LoadOperationStateEnumDto.InProgress,
				Items = new List<OrderItemDto>
				{
					new OrderItemDto
					{
						NomenclatureId = 1,
						Name = "Name1",
						Gtin = "Gtin12345",
						Quantity = 3,
						Codes = new List<TrueMarkCodeDto>
						{
							new TrueMarkCodeDto { SequenceNumber = 1, Code = "TrueMarkCodeDto-1"},
							new TrueMarkCodeDto { SequenceNumber = 2, Code = "TrueMarkCodeDto-2"},
							new TrueMarkCodeDto { SequenceNumber = 3, Code = "TrueMarkCodeDto-3"}
						}
					},
					new OrderItemDto
					{
						NomenclatureId = 2,
						Name = "Name2",
						Gtin = "Gtin123456",
						Quantity = 2,
						Codes = new List<TrueMarkCodeDto>
						{
							new TrueMarkCodeDto { SequenceNumber = 11, Code = "TrueMarkCodeDto-11"},
							new TrueMarkCodeDto { SequenceNumber = 12, Code = "TrueMarkCodeDto-12"}
						}
					},
				}
			};

			return Ok(response);
		}

		/// <summary>
		/// Добавление отсканированного кода маркировки ЧЗ в заказ
		/// </summary>
		/// <param name="codeData"></param>
		/// <returns></returns>
		[HttpPost("AddOrderCode")]
		public async Task<IActionResult> AddOrderCode(AddOrderCodeRequest codeData)
		{
			var response = new AddOrderCodeResponse
			{
				Result = OperationResultEnumDto.Success,
				Nomenclature = new NomenclatureDto
				{
					NomenclatureId = codeData.NomenclatureId,
					Name = "Name3",
					Gtin = "Gtin1234567",
					Quantity = 2,
					Codes = new List<TrueMarkCodeDto>
						{
							new TrueMarkCodeDto { SequenceNumber = 11, Code = "TrueMarkCodeDto-21"},
							new TrueMarkCodeDto { SequenceNumber = 12, Code = codeData.Code}
						}
				}
			};

			return Ok(response);
		}

		/// <summary>
		/// Замена отсканированного кода ЧЗ номенклатуры в заказе
		/// </summary>
		/// <returns></returns>
		[HttpPost("ChangeOrderCode")]
		public async Task<IActionResult> ChangeOrderCode(ChangeOrderCodeRequest requestData)
		{
			var response = new ChangeOrderCodeResponse
			{
				Result = OperationResultEnumDto.Success,
				Nomenclature = new NomenclatureDto
				{
					NomenclatureId = requestData.NomenclatureId,
					Name = "Name3",
					Gtin = "Gtin1234567",
					Quantity = 2,
					Codes = new List<TrueMarkCodeDto>
						{
							new TrueMarkCodeDto { SequenceNumber = 11, Code = "TrueMarkCodeDto-21"},
							new TrueMarkCodeDto { SequenceNumber = 12, Code = requestData.NewCode}
						}
				}
			};

			return Ok(response);
		}

		/// <summary>
		/// Завершение погрузки по талону погрузки
		/// </summary>
		/// <returns></returns>
		[HttpPost("EndLoad")]
		public async Task<IActionResult> EndLoad([FromQuery] int documentId)
		{
			var response = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Success
			};

			return Ok(response);
		}
	}
}
