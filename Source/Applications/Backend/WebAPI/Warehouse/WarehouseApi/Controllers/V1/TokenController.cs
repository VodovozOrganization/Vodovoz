using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Application.FirebaseCloudMessaging;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.WebApi.Security;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер аутентификации
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public class TokenController : AuthenticationController
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeRepository _employeeRepository;

		/// <summary>
		/// Контроллер аутентификации
		/// </summary>
		/// <param name="securityOptions">Настройки безопасности.</param>
		/// <param name="userManager">Менеджер учётных записей приложения.</param>
		/// <param name="firebaseCloudMessagingService">Сервис уведомлений о входе.</param>
		/// <param name="unitOfWork">Контекст работы с данными сотрудников.</param>
		/// <param name="employeeRepository">Репозиторий сотрудников.</param>
		public TokenController(
			IOptions<SecurityOptions> securityOptions,
			UserManager<IdentityUser> userManager,
			IFirebaseCloudMessagingService firebaseCloudMessagingService,
			IUnitOfWork unitOfWork,
			IEmployeeRepository employeeRepository)
			: base(securityOptions, userManager, firebaseCloudMessagingService, unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		/// <inheritdoc />
		protected override bool IsEmployeeFired(string username) =>
			_employeeRepository.GetEmployeeByAndroidLogin(_unitOfWork, username, ExternalApplicationType.WarehouseApp)
				?.Status == EmployeeStatus.IsFired;
	}
}
