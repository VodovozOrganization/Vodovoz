using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Краткие данные контрагента для результата поиска
	/// </summary>
	public class CounterpartySummary
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string Surname { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public PersonType PersonType { get; set; }
		public bool IsArchive { get; set; }
		public bool IsDeliveriesClosed { get; set; }
	}

	/// <summary>
	/// Кандидат точки доставки для поиска по адресу
	/// </summary>
	public class CounterpartyAddressCandidate
	{
		public int Id { get; set; }
		public int CounterpartyId { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string StreetType { get; set; }
		public string Building { get; set; }
		public string Letter { get; set; }
		public string Room { get; set; }
		public RoomType RoomType { get; set; }
		public string Placement { get; set; }
		public string CompiledAddress { get; set; }
		public bool IsActive { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string Surname { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public PersonType PersonType { get; set; }
		public bool IsArchive { get; set; }
		public bool IsDeliveriesClosed { get; set; }
	}

	/// <summary>
	/// Телефон контрагента или точки доставки
	/// </summary>
	public class CounterpartyPhoneInfo
	{
		public int CounterpartyId { get; set; }
		public string Number { get; set; }
		public string DigitsNumber { get; set; }
	}

	/// <summary>
	/// Точка доставки контрагента
	/// </summary>
	public class CounterpartyDeliveryPointInfo
	{
		public int Id { get; set; }
		public int CounterpartyId { get; set; }
		public string CompiledAddress { get; set; }
		public string City { get; set; }
		public string StreetType { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }
		public string Letter { get; set; }
		public string Room { get; set; }
		public RoomType RoomType { get; set; }
		public string Placement { get; set; }
		public bool IsActive { get; set; }
	}

	/// <summary>
	/// Заказ контрагента
	/// </summary>
	public class CounterpartyOrderInfo
	{
		public int Id { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
	}
}
