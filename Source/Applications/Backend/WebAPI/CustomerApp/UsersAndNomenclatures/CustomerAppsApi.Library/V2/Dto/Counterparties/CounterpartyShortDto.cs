using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Краткая информация о контрагенте
	/// </summary>
	public class CounterpartyShortDto
	{
		/// <summary>
		/// Идентификатор клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// Фамилия
		/// </summary>
		public string Surname { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		public string FirstName { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string Patronymic { get; set; }
		/// <summary>
		/// Тип клиента <see cref="PersonType"/>
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PersonType PersonType { get; set; }
		/// <summary>
		/// Тип клиента в текстовом виде
		/// </summary>
		public string PersonTypeName { get; set; }
		/// <summary>
		/// Признак архивного контрагента
		/// </summary>
		public bool IsArchive { get; set; }
		/// <summary>
		/// Признак закрытых поставок контрагента (стоп-лист)
		/// </summary>
		public bool IsDeliveriesClosed { get; set; }
		/// <summary>
		/// Телефоны контрагента
		/// </summary>
		public IReadOnlyList<string> Phones { get; set; } = new List<string>();
		/// <summary>
		/// Точки доставки контрагента
		/// </summary>
		public IReadOnlyList<CounterpartyAddressDto> DeliveryPoints { get; set; } = new List<CounterpartyAddressDto>();
	}
}
