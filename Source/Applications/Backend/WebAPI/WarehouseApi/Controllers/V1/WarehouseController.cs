using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Tools.Store;
using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы со складами
	/// </summary>
	[Route("api/[controller]")]
	public partial class WarehouseController : VersionedController
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IStoreDocumentHelper _storeDocumentHelper;

		/// <summary>
		/// Конструктор контроллера
		/// </summary>
		/// <param name="logger">Логгер для записи логов</param>
		/// <param name="userManager"></param>
		/// <param name="employeeRepository"></param>
		/// <param name="storeDocumentHelper">Помощник для работы с документами склада</param>
		public WarehouseController(
			ILogger<ApiControllerBase> logger,
			UserManager<IdentityUser> userManager,
			IGenericRepository<Employee> employeeRepository,
			IStoreDocumentHelper storeDocumentHelper)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_storeDocumentHelper = storeDocumentHelper
				?? throw new ArgumentNullException(nameof(storeDocumentHelper));
		}

		/// <summary>
		/// Получение списка доступных складов
		/// </summary>
		/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
		/// <returns>Список складов, доступных для просмотра</returns>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
		public IActionResult Get([FromServices] IUnitOfWork unitOfWork)
		{
			var accessibleWarehouses = _storeDocumentHelper.GetRestrictedWarehousesList(unitOfWork, WarehousePermissionsType.WarehouseView);

			var result = accessibleWarehouses
				.Select(warehouse => new WarehouseDto
				{
					Id = warehouse.Id,
					Name = warehouse.Name
				})
				.ToList();

			return Ok(result);
		}

		/// <summary>
		/// Изменение склада по умолчанию в настройках сотрудника
		/// </summary>
		/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
		/// <param name="warehouseId">Идентификатор склада</param>
		/// <returns>Результат изменения склада</returns>
		[HttpPost("ChangeDefaultWarehouse")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> ChangeDefaultWarehouseAsync(
			[FromServices] IUnitOfWork unitOfWork,
			[FromBody] int warehouseId)
		{
			AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var accessTokenValue);
			var accessToken = accessTokenValue?.Parameter ?? string.Empty;
			var user = await _userManager.GetUserAsync(User);

			var employee = _employeeRepository.Get(unitOfWork, x =>x.ExternalApplicationsUsers == user.UserName, 1).FirstOrDefault();
			if(employee == null)
			{
				return BadRequest("Сотрудник с указанным идентификатором не найден.");
			}

			var warehouse = unitOfWork.GetById<Warehouse>(warehouseId);
			if(warehouse == null)
			{
				return BadRequest("Склад с указанным идентификатором не найден.");
			}

			employee.DefaultWarehouse = warehouse;
			unitOfWork.SaveAsync(employee).Wait();
			unitOfWork.CommitAsync().Wait();

			return Ok("Склад по умолчанию успешно изменен.");
		}
	}
}
