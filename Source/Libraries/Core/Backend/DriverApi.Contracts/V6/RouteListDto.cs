namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Маршрутный лист
	/// </summary>
	public class RouteListDto
	{
		/// <summary>
		/// Имя экспедитора
		/// </summary>
		public string ForwarderFullName { get; set; }

		/// <summary>
		/// Статус завершенности
		/// </summary>
		public RouteListDtoCompletionStatus CompletionStatus { get; set; }

		/// <summary>
		/// Если не завершен, содержит информацию по не завершенному маршрутному листу
		/// </summary>
		public IncompletedRouteListDto IncompletedRouteList { get; set; }

		/// <summary>
		/// Если завершен, содержит информацию по завершенному маршрутному листу
		/// </summary>
		public CompletedRouteListDto CompletedRouteList { get; set; }

		/// <summary>
		/// Разрешенное расстояние из ERP
		/// </summary>
		public int PermittedDistance { get; set; }

		/// <summary>
		/// Номер заказа, который выбран водителем следующим для доставки
		/// </summary>
		public int? OpenOrder { get; set; }
	}
}
