using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders.Documents;
using Type = Vodovoz.Domain.Orders.Documents.Type;

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

			DocFlowId = edoContainer.DocFlowId;
			OldEdoDocumentType = edoContainer.Type;
			EdoDocFlowStatus = edoContainer.EdoDocFlowStatus;
			IsReceived = edoContainer.Received;
			ErrorDescription = edoContainer.ErrorDescription;
			IsNewDockflow = false;
		}

		public EdoDockflowData(){ }

		/// <summary>
		/// Идентификатор документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
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
		public Type? OldEdoDocumentType { get; set; }

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
		/// Тип документа
		/// </summary>
		public string DocumentType =>
			IsNewDockflow
			? EdoDocumentType?.GetEnumTitle()
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
