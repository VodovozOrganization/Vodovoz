using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Edo.Upd
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "УПД",
		Nominative = "УПД")]
	public class EdoUpdDocument : PropertyChangedBase, IDomainObject
	{
		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id { get; set; }

		/// <summary>
		/// Уникальный код
		/// </summary>
		[Display(Name = "Уникальный код")]
		public virtual Guid Guid { get; set; }

		/// <summary>
		/// Номер
		/// </summary>
		[Display(Name = "Номер")]
		public virtual string Number { get; set; }

		/// <summary>
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date { get; set; }

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Sum { get; set; }

		/// <summary>
		/// Идентификатор государственного контракта
		/// </summary>
		[Display(Name = "Идентификатор государственного контракта")]
		public virtual string GovContract { get; set; }

		/// <summary>
		/// Продавец
		/// </summary>
		[Display(Name = "Продавец")]
		public virtual UpdOrganization Seller { get; set; } = new UpdOrganization();

		/// <summary>
		/// Грузоотправитель
		/// </summary>
		[Display(Name = "Грузоотправитель")]
		public virtual UpdOrganization Shipper { get; set; } = new UpdOrganization();

		/// <summary>
		/// Покупатель
		/// </summary>
		[Display(Name = "Покупатель")]
		public virtual UpdOrganization Customer { get; set; } = new UpdOrganization();

		/// <summary>
		/// Грузополучатель
		/// </summary>
		[Display(Name = "Грузополучатель")]
		public virtual UpdOrganization Consignee { get; set; } = new UpdOrganization();

		/// <summary>
		/// Особый грузополучатель
		/// </summary>
		[Display(Name = "Особый грузополучатель")]
		public virtual string SpecialConsignee { get; set; }

		/// <summary>
		/// Документ подтверждающий отгрузку
		/// </summary>
		[Display(Name = "Документ подтверждающий отгрузку")]
		public virtual string ShippingDocument { get; set; }

		/// <summary>
		/// Номер документа отгрузки
		/// </summary>
		[Display(Name = "Номер документа отгрузки")]
		public virtual string ShippingDocumentNumber { get; set; }

		/// <summary>
		/// Дата документа отгрузки
		/// </summary>
		[Display(Name = "Дата документа отгрузки")]
		public virtual string ShippingDocumentDate { get; set; }

		/// <summary>
		/// Документ основания отгрузки
		/// </summary>
		[Display(Name = "Документ основания отгрузки")]
		public virtual string ShippingReasonDocument { get; set; }

		/// <summary>
		/// Номер документа основания отгрузки
		/// </summary>
		[Display(Name = "Номер документа основания отгрузки")]
		public virtual string ShippingReasonDocumentNumber { get; set; }

		/// <summary>
		/// Дата документа основания отгрузки
		/// </summary>
		[Display(Name = "Дата документа основания отгрузки")]
		public virtual string ShippingReasonDocumentDate { get; set; }

		/// <summary>
		/// Причина выбытия товара
		/// </summary>
		[Display(Name = "Причина выбытия товара")]
		public virtual ReasonForLeaving ReasonForLeaving { get; set; }

		/// <summary>
		/// Номер заказа
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual int OrderId { get; set; }

		/// <summary>
		/// Идентификатор заказа в ИС контрагента
		/// </summary>
		[Display(Name = "Идентификатор заказа в ИС контрагента")]
		public virtual string CustomerExternalOrderId { get; set; }

		/// <summary>
		/// Оплаты
		/// </summary>
		[Display(Name = "Оплаты")]
		public virtual IList<UpdPayment> Payments { get; set; } = new List<UpdPayment>();

		/// <summary>
		/// Товары
		/// </summary>
		[Display(Name = "Товары")]
		public virtual IList<UpdInventPosition> InventPositions { get; set; } = new List<UpdInventPosition>();
	}
}
