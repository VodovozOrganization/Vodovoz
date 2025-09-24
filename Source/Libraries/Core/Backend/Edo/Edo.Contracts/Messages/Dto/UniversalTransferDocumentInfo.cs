using System;
using System.Collections.Generic;

namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Данные для УПД по ЭДО
	/// </summary>
	public class UniversalTransferDocumentInfo : DocumentInfo
	{
		/// <summary>
		/// Номер документа
		/// </summary>
		public int Number { get; set; }
		/// <summary>
		/// Сумма документа
		/// </summary>
		public decimal Sum { get; set; }
		/// <summary>
		/// Дата документа
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// Продавец <see cref="SellerInfo"/>
		/// </summary>
		public SellerInfo Seller { get; set; }
		/// <summary>
		/// Грузоотправитель <see cref="ShipperInfo"/>
		/// Не обязателен для заполнения, если это та же организация, что и Продавец
		/// </summary>
		public ShipperInfo Shipper { get; set; }
		/// <summary>
		/// Покупатель <see cref="CustomerInfo"/>
		/// </summary>
		public CustomerInfo Customer { get; set; }
		/// <summary>
		/// Грузополучатель <see cref="ConsigneeInfo"/>
		/// </summary>
		public ConsigneeInfo Consignee { get; set; }
		/// <summary>
		/// Документ подтверждающий отгрузку <see cref="DocumentConfirmingShipmentInfo"/>
		/// </summary>
		public DocumentConfirmingShipmentInfo DocumentConfirmingShipment { get; set; }
		/// <summary>
		/// Основание отгрузки <see cref="BasisShipmentInfo"/>
		/// </summary>
		public BasisShipmentInfo BasisShipment { get; set; }
		/// <summary>
		/// Идентификатор государственного контракта, договора
		/// </summary>
		public string GovContract { get; set; }
		/// <summary>
		/// Информация об оплатах <see cref="PaymentInfo"/>
		/// </summary>
		public IEnumerable<PaymentInfo> Payments { get; set; }
		/// <summary>
		/// Информация о товарах заказа <see cref="ProductInfo"/>
		/// </summary>
		public IEnumerable<ProductInfo> Products { get; set; }
		/// <summary>
		/// Дополнительные параметры УПД <see cref="UpdAdditionalInfo"/>
		/// </summary>
		public IEnumerable<UpdAdditionalInfo> AdditionalInformation { get; set; }
	}
}
