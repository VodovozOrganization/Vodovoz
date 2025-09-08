using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Extensions;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права сотрудники
	/// </summary>
	public static partial class EmployeePermissions
	{
		/// <summary>
		/// Может устанавливать клиента в карточке сотрудника
		/// </summary>
		[Display(
			Name = "Может устанавливать клиента в карточке сотрудника",
			Description = "Может устанавливать клиента в карточке сотрудника для работы с фиксой сотрудников ВВ")]
		public static string CanChangeEmployeeCounterparty => $"Employee.{nameof(CanChangeEmployeeCounterparty)}";
		
		/// <summary>
		/// Изменение существующего и создание нового пользователей в диалоге сотрудника
		/// </summary>
		[Display(Name = "Изменение существующего и создание нового пользователей в диалоге сотрудника")]
		public static string CanManageUsers => $"{nameof(CanManageUsers).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Возможность активировать версии приоритетов районов доставки в карточке сотрудника
		/// </summary>
		[Display(
			Name = "Возможность активировать версии приоритетов районов доставки",
			Description = "Возможность активировать версии приоритетов районов доставки")]
		public static string CanActivateDriverDistrictPrioritySet =>
			$"{nameof(CanActivateDriverDistrictPrioritySet).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Создание и изменение водителей и экспедиторов
		/// </summary>
		[Display(Name = "Создание и изменение водителей и экспедиторов")]
		public static string CanManageDriversAndForwarders =>
			$"{nameof(CanManageDriversAndForwarders).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Создание и изменение офисных работников
		/// </summary>
		[Display(Name = "Создание и изменение офисных работников")]
		public static string CanManageOfficeWorkers => $"{nameof(CanManageOfficeWorkers).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Установка параметров заработной платы
		/// </summary>
		[Display(
			Name = "Установка заработной платы",
			Description = "Пользователь может устанавливать тип заработной платы и ставку")]
		public static string CanEditWage => $"{nameof(CanEditWage).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Пользователь может менять организацию для з/п в карточке сотрудника
		/// </summary>
		[Display(Name = "Пользователь может менять организацию для з/п в карточке сотрудника")]
		public static string CanEditOrganisationForSalary => $"{nameof(CanEditOrganisationForSalary).FromPascalCaseToSnakeCase()}";
		
		/// <summary>
		/// Ограничение работы только с документами водителей и экспедиторов
		/// </summary>
		[Display(
			Name = "Работа с документами сотрудников ограничена только водителями и экспедиторами",
			Description = "Если включено, то можно просматривать/редактировать/создавать только документы у водителей и экспедиторов")]
		public static string CanWorkWithOnlyDriverDocuments => "work_with_only_driver_documents";
	}
}
