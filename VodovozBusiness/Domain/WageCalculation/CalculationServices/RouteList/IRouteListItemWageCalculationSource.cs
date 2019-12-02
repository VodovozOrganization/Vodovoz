using System;
using System.Collections.Generic;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	/// <summary>
	/// Предоставляет данные необходимые для расчета зарплаты за адрес
	/// </summary>
	public interface IRouteListItemWageCalculationSource
	{
		/// <summary>
		/// Количество полных 19л бутылей
		/// </summary>
		int FullBottle19LCount { get; }

		/// <summary>
		/// Количество пустых 19л бутылей
		/// </summary>
		int EmptyBottle19LCount { get; }

		/// <summary>
		/// Количество 6л бутылей
		/// </summary>
		int Bottle6LCount { get; }

		/// <summary>
		/// Количество 0,6л бутылей
		/// </summary>
		int Bottle600mlCount { get; }

		/// <summary>
		/// Заказ на адресе является расторжением договора
		/// </summary>
		bool ContractCancelation{ get; }

		/// <summary>
		/// Надбавка водителю за адрес
		/// </summary>
		decimal DriverWageSurcharge { get; }

		/// <summary>
		/// Указывает на то, первый ли это заказ среди других
		/// <see cref="T:Vodovoz.Domain.WageCalculation.CalculationServices.RouteList.IRouteListItemWageCalculationSource"/>
		/// на ту же самую точку доставки. Т.е. если в МЛ два или более заказов 
		/// на одну точку доставки, то для первого заказа должно быть возвращено
		/// <c>true</c>, а для остальных <c>false</c>.
		/// </summary>
		/// <value><c>true</c> если это первый или единственный заказ на адрес,
		/// <c>false</c> в противном случае.</value>
		bool HasFirstOrderForDeliveryPoint { get; }

		/// <summary>
		/// Зарплатный район к которому относится данный адрес
		/// </summary>
		/// <value>Зарплатный район адреса</value>
		WageDistrict WageDistrictOfAddress { get; }

		/// <summary>
		/// Был ли адрес посещён с экспедитором
		/// </summary>
		bool WasVisitedByForwarder { get; }

		/// <summary>
		/// Требуется доставить или забрать оборудование
		/// </summary>
		bool NeedTakeOrDeliverEquipment { get; }

		/// <summary>
		/// Методика расчёта ЗП. Заполняется для исторических МЛ, т.к. районы
		/// доставки (<see cref="Sale.ScheduleRestrictedDistrict"/>) со временем
		/// могут меняться и происходить смена зарплатной группы
		/// (<see cref="WageDistrict"/>), что недопустимо, ибо произойдёт
		/// перерасчёт ЗП.
		/// </summary>
		WageDistrictLevelRate WageCalculationMethodic { get; }

		/// <summary>
		/// Доставелен ли заказ по адресу
		/// </summary>
		/// <value><c>true</c> if is delivered; otherwise, <c>false</c>.</value>
		bool IsDelivered { get; }

		/// <summary>
		/// Время доставки
		/// </summary>
		(TimeSpan,TimeSpan) DeliverySchedule { get; }

		IEnumerable<IOrderItemWageCalculationSource> OrderItemsSource { get; }
		IEnumerable<IOrderDepositItemWageCalculationSource> OrderDepositItemsSource { get; }
	}
}