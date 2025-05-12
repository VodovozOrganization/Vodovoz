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
using Vodovoz.Domain.Store;
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
		private readonly IGenericRepository<ExternalApplicationUser> _externalApplicationUserRepository;
		private readonly IGenericRepository<UserSettings> _userSettingsRepository;
		private readonly IStoreDocumentHelper _storeDocumentHelper;

		/// <summary>
		/// Конструктор контроллера
		/// </summary>
		/// <param name="logger">Логгер для записи логов</param>
		/// <param name="userManager"></param>
		/// <param name="employeeRepository"></param>
		/// <param name="externalApplicationUserRepository"></param>
		/// <param name="userSettingsRepository"></param>
		/// <param name="storeDocumentHelper">Помощник для работы с документами склада</param>
		public WarehouseController(
			ILogger<ApiControllerBase> logger,
			UserManager<IdentityUser> userManager,
			IGenericRepository<Employee> employeeRepository,
			IGenericRepository<ExternalApplicationUser> externalApplicationUserRepository,
			IGenericRepository<UserSettings> userSettingsRepository,
			IStoreDocumentHelper storeDocumentHelper)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_externalApplicationUserRepository = externalApplicationUserRepository
				?? throw new ArgumentNullException(nameof(externalApplicationUserRepository));
			_userSettingsRepository = userSettingsRepository
				?? throw new ArgumentNullException(nameof(userSettingsRepository));
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

			var apiUser = _externalApplicationUserRepository.Get(
				unitOfWork,
				x => x.Login == user.UserName
					&& x.ExternalApplicationType == Vodovoz.Core.Domain.Employees.ExternalApplicationType.WarehouseApp,
				1)
				.FirstOrDefault();

			if(apiUser == null)
			{
				return Problem("Сотрудник не имеет доступа к этмоу Api", statusCode: StatusCodes.Status403Forbidden);
			}

			var employee = apiUser.Employee;

			if(employee == null)
			{
				return Problem("Сотрудник не найден", statusCode: StatusCodes.Status403Forbidden);
			}

			var warehouse = unitOfWork.GetById<Warehouse>(warehouseId);
			
			if(warehouse == null)
			{
				return BadRequest("Склад с указанным идентификатором не найден.");
			}

			var availableWarehouses = _storeDocumentHelper.GetRestrictedWarehousesIds(unitOfWork, WarehousePermissionsType.WarehouseView);

			if(!availableWarehouses.Contains(warehouseId))
			{
				return Problem("У вас нет прав на доступ к этому складу", statusCode: StatusCodes.Status403Forbidden);
			}

			var userSettings = _userSettingsRepository
				.Get(unitOfWork, x => x.User.Id == employee.User.Id, 1)
				.FirstOrDefault();

			if(userSettings == null)
			{
				return Problem("Настройки пользователя не найдены", statusCode: StatusCodes.Status500InternalServerError);
			}

			userSettings.DefaultWarehouse = warehouse;

			await unitOfWork.SaveAsync(userSettings);
			await unitOfWork.CommitAsync();

			return NoContent();
		}
	}
}
