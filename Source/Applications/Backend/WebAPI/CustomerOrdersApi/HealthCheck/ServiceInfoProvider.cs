using VodovozHealthCheck.Providers;

namespace CustomerOrdersApi.HealthCheck
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription =>
			"Проверка возможности применения и применение промокода. " +
			"Оповещение пользователя о применимости промокода. " +
			"Применение фиксы. " +
			"Регистрация оценки заказа. " +
			"Получение всех причин оценки заказа. " +
			"Регистрация заказа. " +
			"Получение детальной информации о заказе. " +
			"Получение заказов клиента. " +
			"Создание заявки на звонок.";

		public string Name => "Сервис регистрации онлайн заказов, заявок на звонок";
	}
}
