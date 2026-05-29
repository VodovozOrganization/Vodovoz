using System;

namespace Vodovoz.Settings.Logistics
{
	public interface IDriverApiSettings
	{
		/// <summary>
		/// Номер телефона компании
		/// </summary>
		string CompanyPhoneNumber { get; }

		/// <summary>
		/// Идентификатор источника рекламации
		/// </summary>
		int ComplaintSourceId { get; }

		/// <summary>
		/// Базовый URL API для взаимодействия с водительским приложением
		/// </summary>
		Uri ApiBase { get; }

		/// <summary>
		/// URI для уведомления об изменении статуса оплаты по SMS
		/// </summary>
		string NotifyOfSmsPaymentStatusChangedUri { get; }

		/// <summary>
		/// URI для уведомления о добавлении заказа с ДЗЧ
		/// </summary>
		string NotifyOfFastDeliveryOrderAddedUri { get; }

		/// <summary>
		/// URI для уведомления об изменении времени ожидания
		/// </summary>
		string NotifyOfWaitingTimeChangedURI { get; }

		/// <summary>
		/// URI для уведомления об изменении маршрутного листа
		/// </summary>
		string NotifyOfRouteListChangedUri { get; }

		/// <summary>
		/// URI для уведомления о выдаче заявки на выдачу наличных водителю
		/// </summary>
		string NotifyOfCashRequestForDriverIsGivenForTakeUri { get; }

		/// <summary>
		/// Идентификатор пользователя API для водительского приложения
		/// </summary>
		int DriverApiUserId { get; }

		/// <summary>
		/// Разрешенное расстояние из ERP
		/// </summary>
		int PermittedDistance { get; }

		/// <summary>
		/// Сохранить разрешенное расстояние из ERP
		/// </summary>
		/// <param name="permittedDistance">Разрешенное расстояние в метрах</param>
		void SavePermittedDistance(int permittedDistance);
	}
}
