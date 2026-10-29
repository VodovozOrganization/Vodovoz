using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.SiteOrdersImport
{
	/// <summary>
	/// Запись выгрузки с сайта: заказ или брошенная корзина.
	/// Полезная нагрузка хранится сырым JSON по контракту v1; идемпотентность обеспечивается
	/// уникальностью пары «идентификатор записи на сайте + тип сущности».
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "запись выгрузки с сайта",
		NominativePlural = "записи выгрузки с сайта")]
	public class SiteOrderImportItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private long _siteOrderId;
		private string _entityType;
		private string _siteStatus;
		private string _siteUpdatedAt;
		private string _batchId;
		private string _contractVersion;
		private string _sentAt;
		private string _payload;
		private DateTime _receivedAt;

		/// <summary>
		/// Идентификатор.
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Идентификатор записи на стороне сайта (order_id; для корзины — идентификатор корзины).
		/// </summary>
		[Display(Name = "ID записи на сайте")]
		public virtual long SiteOrderId
		{
			get => _siteOrderId;
			set => SetField(ref _siteOrderId, value);
		}

		/// <summary>
		/// Тип сущности: order (заказ) или abandoned_cart (брошенная корзина).
		/// </summary>
		[Display(Name = "Тип сущности")]
		public virtual string EntityType
		{
			get => _entityType;
			set => SetField(ref _entityType, value);
		}

		/// <summary>
		/// Статус записи на стороне сайта.
		/// </summary>
		[Display(Name = "Статус на сайте")]
		public virtual string SiteStatus
		{
			get => _siteStatus;
			set => SetField(ref _siteStatus, value);
		}

		/// <summary>
		/// Время последнего изменения записи на стороне сайта (как пришло, формат "Y-m-d H:i:s").
		/// </summary>
		[Display(Name = "Изменено на сайте")]
		public virtual string SiteUpdatedAt
		{
			get => _siteUpdatedAt;
			set => SetField(ref _siteUpdatedAt, value);
		}

		/// <summary>
		/// Идентификатор пакета, в котором запись была получена.
		/// </summary>
		[Display(Name = "ID пакета")]
		public virtual string BatchId
		{
			get => _batchId;
			set => SetField(ref _batchId, value);
		}

		/// <summary>
		/// Версия контракта данных (например, "v1").
		/// </summary>
		[Display(Name = "Версия контракта")]
		public virtual string ContractVersion
		{
			get => _contractVersion;
			set => SetField(ref _contractVersion, value);
		}

		/// <summary>
		/// Время формирования пакета на стороне сайта (как пришло).
		/// </summary>
		[Display(Name = "Сформировано на сайте")]
		public virtual string SentAt
		{
			get => _sentAt;
			set => SetField(ref _sentAt, value);
		}

		/// <summary>
		/// Полезная нагрузка записи по контракту v1, сырой JSON.
		/// </summary>
		[Display(Name = "Полезная нагрузка")]
		public virtual string Payload
		{
			get => _payload;
			set => SetField(ref _payload, value);
		}

		/// <summary>
		/// Время приёма записи на нашей стороне.
		/// </summary>
		[Display(Name = "Время приёма")]
		public virtual DateTime ReceivedAt
		{
			get => _receivedAt;
			set => SetField(ref _receivedAt, value);
		}
	}
}
