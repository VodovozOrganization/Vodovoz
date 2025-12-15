using Microsoft.AspNetCore.Authorization;
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
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Contracts.V1.Requests;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы со складами
	/// </summary>
	//[Authorize(Roles = _rolesToAccess)]
	[OnlyOneSession]
	[Route("api/[controller]")]
	public partial class WarehouseController : VersionedController
	{
		private const string _rolesToAccess = nameof(ApplicationUserRole.WarehousePicker);

		private readonly UserManager<IdentityUser> _userManager;
		private readonly IGenericRepository<UserSettings> _userSettingsRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IGenericRepository<ExternalApplicationUser> _externalApplicationUserRepository;
		private readonly IWarehousePermissionService _warehousePermissionService;

		/// <summary>
		/// Конструктор контроллера
		/// </summary>
		/// <param name="logger">Логгер для записи логов</param>
		/// <param name="userManager"></param>
		/// <param name="userSettingsRepository"></param>
		/// <param name="externalApplicationUserRepository"></param>
		/// <param name="warehousePermissionService"></param>
		/// <param name="employeeRepository"></param>
		public WarehouseController(
			ILogger<ApiControllerBase> logger,
			UserManager<IdentityUser> userManager,
			IGenericRepository<UserSettings> userSettingsRepository,
			IGenericRepository<ExternalApplicationUser> externalApplicationUserRepository,
			IWarehousePermissionService warehousePermissionService,
			IGenericRepository<Employee> employeeRepository)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_userSettingsRepository = userSettingsRepository
				?? throw new ArgumentNullException(nameof(userSettingsRepository));
			_externalApplicationUserRepository = externalApplicationUserRepository
				?? throw new ArgumentNullException(nameof(externalApplicationUserRepository));
			_warehousePermissionService = warehousePermissionService
				?? throw new ArgumentNullException(nameof(warehousePermissionService));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		/// <summary>
		/// Получение списка доступных складов
		/// </summary>
		/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
		/// <returns>Список складов, доступных для просмотра</returns>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetAsync([FromServices] IUnitOfWork unitOfWork)
		{
			AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var accessTokenValue);
			var accessToken = accessTokenValue?.Parameter ?? string.Empty;
			var user = await _userManager.GetUserAsync(User);

			var apiUser = _externalApplicationUserRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => x.Login == user.UserName
						&& x.ExternalApplicationType == ExternalApplicationType.WarehouseApp);

			if(apiUser == null)
			{
				return Problem("Сотрудник не имеет доступа к этмоу Api", statusCode: StatusCodes.Status403Forbidden);
			}

			var employee = _employeeRepository.GetFirstOrDefault(unitOfWork, x => x.Id == apiUser.Employee.Id);

			if(employee == null)
			{
				return Problem("Сотрудник не найден", statusCode: StatusCodes.Status403Forbidden);
			}

			if(employee.User is null)
			{
				return Problem("Пользователь не найден", statusCode: StatusCodes.Status403Forbidden);
			}

			var accessibleWarehouses = _warehousePermissionService
				.GetAvailableWarehousesForUser(unitOfWork, employee.User.Id);

			var defaultWarehouseId = (_userSettingsRepository
				.GetFirstOrDefault(unitOfWork, x => x.User.Id == employee.User.Id))
				?.DefaultWarehouse?.Id;

			var result = accessibleWarehouses
				.Select(warehouse => new WarehouseDto
				{
					Id = warehouse.Id,
					Name = warehouse.Name,
					IsDefault = warehouse.Id == defaultWarehouseId
				})
				.ToList();

			return Ok(result);
		}

		/// <summary>
		/// Изменение склада по умолчанию в настройках сотрудника
		/// </summary>
		/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
		/// <param name="warehouse">Склад</param>
		/// <returns>Результат изменения склада</returns>
		[HttpPost("ChangeDefaultWarehouse")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> ChangeDefaultWarehouseAsync(
			[FromServices] IUnitOfWork unitOfWork,
			ChangeDefaultWarehouseRequest warehouse)
		{
			AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var accessTokenValue);
			var accessToken = accessTokenValue?.Parameter ?? string.Empty;
			var user = await _userManager.GetUserAsync(User);

			var apiUser = _externalApplicationUserRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => x.Login == user.UserName
						&& x.ExternalApplicationType == ExternalApplicationType.WarehouseApp);

			if(apiUser == null)
			{
				return Problem("Сотрудник не имеет доступа к этмоу Api", statusCode: StatusCodes.Status403Forbidden);
			}

			var employee = _employeeRepository.GetFirstOrDefault(unitOfWork, x => x.Id == apiUser.Employee.Id);

			if(employee == null)
			{
				return Problem("Сотрудник не найден", statusCode: StatusCodes.Status403Forbidden);
			}

			if(employee.User is null)
			{
				return Problem("Пользователь не найден", statusCode: StatusCodes.Status403Forbidden);
			}

			var accessibleWarehouses = _warehousePermissionService
				.GetAvailableWarehousesForUser(unitOfWork, employee.User.Id);

			if(!accessibleWarehouses.Any(x => x.Id == warehouse.WarehouseId))
			{
				return Problem("У вас нет прав на доступ к этому складу", statusCode: StatusCodes.Status403Forbidden);
			}

			var userSettings = _userSettingsRepository
				.GetFirstOrDefault(unitOfWork, x => x.User.Id == employee.User.Id);

			if(userSettings == null)
			{
				return Problem("Настройки пользователя не найдены", statusCode: StatusCodes.Status500InternalServerError);
			}

			userSettings.DefaultWarehouse = new Warehouse { Id = warehouse.WarehouseId };

			await unitOfWork.SaveAsync(userSettings);
			await unitOfWork.CommitAsync();

			return NoContent();
		}
	}
}
