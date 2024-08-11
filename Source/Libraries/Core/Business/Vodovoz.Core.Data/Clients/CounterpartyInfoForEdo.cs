using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Goods;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.Clients
{
	/// <summary>
	/// Информация о клиенте для ЭДО(электронного документооборота)
	/// </summary>
	public class CounterpartyInfoForEdo
	{
		/// <summary>
		/// Id клиента
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Личный кабинет в ЭДО
		/// </summary>
		public string PersonalAccountIdInEdo { get; set; }
		/// <summary>
		/// Юридический адрес
		/// </summary>
		public string JurAddress { get; set; }
		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string INN { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string KPP { get; set; }
		/// <summary>
		/// Использовать спец поля
		/// </summary>
		public bool UseSpecialDocFields { get; set; }
		/// <summary>
		/// Особое название договора
		/// </summary>
		public string SpecialContractName { get; set; }
		/// <summary>
		/// Особый номер договора
		/// </summary>
		public string SpecialContractNumber { get; set; }
		/// <summary>
		/// Особая дата договора
		/// </summary>
		public DateTime? SpecialContractDate { get; set; }
		/// <summary>
		/// Особый 
		/// </summary>
		public string SpecialCustomer { get; set; }
		/// <summary>
		/// Особое КПП плательщика
		/// </summary>
		public string PayerSpecialKPP { get; set; }
		/// <summary>
		/// Особый грузополучатель
		/// </summary>
		public string CargoReceiver { get; set; }
		/// <summary>
		/// Источник получения данных о грузополучателе
		/// </summary>
		public CargoReceiverSource CargoReceiverSource { get; set; }
		/// <summary>
		/// Тип клиента
		/// </summary>
		public PersonType PersonType { get; set; }
		/// <summary>
		/// Причина покупки воды
		/// </summary>
		public ReasonForLeaving ReasonForLeaving { get; set; }
		/// <summary>
		/// Список спец номенклатур
		/// </summary>
		public IList<SpecialNomenclatureInfoForEdo> SpecialNomenclatures { get; set; }
	}
}
