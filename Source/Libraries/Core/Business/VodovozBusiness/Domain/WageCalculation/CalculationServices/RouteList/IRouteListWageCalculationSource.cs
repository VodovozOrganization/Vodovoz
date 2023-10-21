using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	/// <summary>
	/// Предоставляет данные необходимые для расчета зарплаты за маршрутный лист
	/// </summary>
	public interface IRouteListWageCalculationSource
	{
		int RouteListId { get; }

		/// <summary>
		/// Итоговая сумма за все адреса в МЛ
		/// </summary>
		decimal TotalSum { get; }

		/// <summary>
		/// Наличие в маршрутном листе хотябы одного завершенного адреса
		/// </summary>
		bool HasAnyCompletedAddress { get; }

		EmployeeCategory EmployeeCategory { get; }

		DateTime RouteListDate { get; }

		/// <summary>
		/// Водитель использует в МЛ наш автомобиль
		/// </summary>
		bool DriverOfOurCar { get; }

		/// <summary>
		/// Раскатный автомобиль
		/// </summary>
		bool IsRaskatCar { get; }

		CarTypeOfUse CarTypeOfUse { get; }

		/// <summary>
		/// Фура
		/// </summary>
		bool IsTruck { get; }

		decimal FixedWage { get; }

		/// <summary>
		/// Список данных для расчета зарплаты по каждому адресу
		/// </summary>
		/// <value>The item sources.</value>
		IEnumerable<IRouteListItemWageCalculationSource> ItemSources { get; }
	}
}
