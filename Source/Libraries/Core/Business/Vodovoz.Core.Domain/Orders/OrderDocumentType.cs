using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum OrderDocumentType
	{
		[Display(Name = "Доп. соглашение для заказа")]
		AdditionalAgreement,
		[Display(Name = "Договор для заказа")]
		Contract,
		[Display(Name = "Доверенность М-2 для заказа")]
		M2Proxy,
		[DocumentOfOrder]
		[Display(Name = "Счет")]
		Bill,
		[DocumentOfOrder]
		[Display(Name = "Особый счет")]
		SpecialBill,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на долг")]
		BillWSForDebt,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на предоплату")]
		BillWSForAdvancePayment,
		[DocumentOfOrder]
		[Display(Name = "Счет без отгрузки на постоплату")]
		BillWSForPayment,
		[DocumentOfOrder]
		[Display(Name = "Акт выполненных работ")]
		DoneWorkReport,
		[DocumentOfOrder]
		[Display(Name = "Акт приема-передачи оборудования")]
		EquipmentTransfer,
		[DocumentOfOrder]
		[Display(Name = "Акт закрытия аренды")]
		EquipmentReturn,
		[DocumentOfOrder]
		[Display(Name = "Накладная (нал.)")]
		Invoice,
		[DocumentOfOrder]
		[Display(Name = "Накладная (безденежно)")]
		InvoiceBarter,
		[DocumentOfOrder]
		[Display(Name = "Накладная (контрактная документация)")]
		InvoiceContractDoc,
		[DocumentOfOrder]
		[Display(Name = "УПД")]
		UPD,
		[DocumentOfOrder]
		[Display(Name = "Особый УПД")]
		SpecialUPD,
		[DocumentOfOrder]
		[Display(Name = "Талон водителю")]
		DriverTicket,
		[DocumentOfOrder]
		[Display(Name = "ТОРГ-12")]
		Torg12,
		[DocumentOfOrder]
		[Display(Name = "Счет-Фактура")]
		ShetFactura,
		[Display(Name = "Сертификат продукции")]
		ProductCertificate,
		[DocumentOfOrder]
		[Display(Name = "Товарно-транспортная накладная")]
		TransportInvoice,
		[DocumentOfOrder]
		[Display(Name = "ТОРГ-2")]
		Torg2,
		[DocumentOfOrder]
		[Display(Name = "Лист сборки")]
		AssemblyList
	}
}
