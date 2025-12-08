using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные документооборота
	/// </summary>
	public class EdoDockflowData
	{
		public EdoDockflowData(EdoContainer edoContainer)
		{
			if(edoContainer is null)
			{
				throw new ArgumentNullException(nameof(edoContainer));
			}

			OrderId = edoContainer.Order?.Id;
			DocFlowId = edoContainer.DocFlowId;
			OldEdoDocumentType = edoContainer.Type;
			EdoDocFlowStatus = edoContainer.EdoDocFlowStatus;
			IsReceived = edoContainer.Received;
			ErrorDescription = edoContainer.ErrorDescription;
			OrderWithoutShipmentForAdvancePaymentId = edoContainer.OrderWithoutShipmentForAdvancePayment?.Id;
			OrderWithoutShipmentForDebtId = edoContainer.OrderWithoutShipmentForDebt?.Id;
			OrderWithoutShipmentForPaymentId = edoContainer.OrderWithoutShipmentForPayment?.Id;
			TaxcomDocflowCreationTime = edoContainer.Created;
			IsNewDockflow = false;
		}

		public EdoDockflowData(){ }

		/// <summary>
		/// Номер заказа
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Идентификатор документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }

		/// <summary>
		/// Дата создания документооборота
		/// </summary>
		public DateTime? TaxcomDocflowCreationTime { get; set; }

		/// <summary>
		/// Статус документооборота
		/// </summary>
		public EdoDocFlowStatus? EdoDocFlowStatus { get; set; }

		/// <summary>
		/// Доставлен
		/// </summary>
		public bool IsReceived { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }

		/// <summary>
		/// Новый документооборот
		/// </summary>
		public bool IsNewDockflow { get; set; }

		/// <summary>
		/// Тип документа в старом документообороте
		/// </summary>
		public DocumentContainerType? OldEdoDocumentType { get; set; }

		/// <summary>
		/// Номер счета без отгрузки на предоплату
		/// </summary>
		public int? OrderWithoutShipmentForAdvancePaymentId { get; set; }

		/// <summary>
		/// Номер счета без отгрузки на долг
		/// </summary>
		public int? OrderWithoutShipmentForDebtId { get; set; }

		/// <summary>
		/// Номер счета без отгрузки на постоплату
		/// </summary>
		public int? OrderWithoutShipmentForPaymentId { get; set; }

		/// <summary>
		/// Тип документа в новом документообороте
		/// </summary>
		public EdoDocumentType? EdoDocumentType { get; set; }

		/// <summary>
		/// Статус задачи ЭДО в новом документообороте
		/// </summary>
		public EdoTaskStatus? EdoTaskStatus { get; set; }

		/// <summary>
		/// Статус документа ЭДО в новом документообороте
		/// </summary>
		public EdoDocumentStatus? EdoDocumentStatus { get; set; }

		/// <summary>
		/// Тип документа заказа
		/// </summary>
		public OrderDocumentType? OrderDocumentType { get; set; }

		/// <summary>
		/// Дата создания запроса ЭДО
		/// </summary>
		public DateTime? EdoRequestCreationTime { get; set; }

		/// <summary>
		/// Тип документа
		/// </summary>
		public string DocumentType =>
			IsNewDockflow
				? (EdoDocumentType != null && EdoDocumentType == Vodovoz.Core.Domain.Edo.EdoDocumentType.InformalOrderDocument
					? OrderDocumentType?.GetEnumTitle()
					: EdoDocumentType?.GetEnumTitle())
				: OldEdoDocumentType?.GetEnumTitle();

		/// <summary>
		/// Статус задачи ЭДО
		/// </summary>
		public string EdoDocFlowStatusString =>
			EdoDocFlowStatus is null
			? string.Empty
			: EdoDocFlowStatus.Value.GetEnumTitle();
	}
}
