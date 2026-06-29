using Gamma.Utilities;
using QS.ViewModels;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentHistoryRowViewModel : ViewModelBase
	{
		public EdoInOrderDocumentHistoryRowViewModel(EdoInOrderDocumentNode documentNode)
		{
			Document = documentNode;

			Time = documentNode.RequestTime;
			TimeString = Time.ToString("dd.MM.yyyy HH:mm");
			Source = documentNode.RequestSource;
			SourceString = Source.GetEnumTitle();
			Status = documentNode.TaskStatus;
			StatusString = Status.GetEnumTitle();
			CodesQuantity = documentNode.CodesQuantity;
			CodesQuantityString = CodesQuantity?.ToString() ?? "-";
			DocumentType = MatchDocumentType(documentNode);
			DocumentTypeString = DocumentType.GetEnumTitle();
			DocumentGroupType = MatchDocumentsByGroupType(DocumentType);
			DocumentGroupTypeString = DocumentGroupType.GetEnumTitle();
		}
		public EdoInOrderDocumentNode Document { get; }

		public virtual EdoInOrderDocumentGroupType DocumentGroupType { get; }
		public virtual string DocumentGroupTypeString { get; }

		public virtual DateTime Time { get; }
		public virtual string TimeString { get; }

		public virtual EdoRequestSource Source { get; }
		public virtual string SourceString { get; }

		public virtual EdoTaskStatus Status { get; }
		public virtual string StatusString { get; }

		public virtual EdoInOrderDocumentType DocumentType { get; }
		public virtual string DocumentTypeString { get; }

		public virtual int? CodesQuantity { get; }
		public virtual string CodesQuantityString { get; }


		private EdoInOrderDocumentType MatchDocumentType(EdoInOrderDocumentNode document)
		{
			switch(document.TaskType)
			{
				case EdoTaskType.Document:
					return EdoInOrderDocumentType.Upd;
				case EdoTaskType.Receipt:
					return EdoInOrderDocumentType.Receipt;
				case EdoTaskType.InformalOrderDocument:
					return MatchInformalDocuments(document);
				case EdoTaskType.SaveCode:
					return EdoInOrderDocumentType.SaveCode;
				case EdoTaskType.Withdrawal:
					return EdoInOrderDocumentType.Withdrawal;
				case EdoTaskType.Tender:
					return EdoInOrderDocumentType.Tender;
				case EdoTaskType.Transfer:
				case EdoTaskType.BulkAccounting:
					throw new InvalidOperationException($"Задача {document.TaskType} не может использоваться тут");
				default:
					throw new NotSupportedException($"Неизвестный тип задачи: {document.TaskType}.");
			}
		}

		private EdoInOrderDocumentType MatchInformalDocuments(EdoInOrderDocumentNode document)
		{
			if(document.InformalOrderDocumentType == null)
			{
				throw new NotImplementedException($"Пока еще не реализованы не формализованные документы без заказа");
			}

			switch(document.InformalOrderDocumentType.Value)
			{
				case OrderDocumentType.Bill:
					return EdoInOrderDocumentType.Bill;
				default:
					throw new NotSupportedException($"Не поддерживается отправка документа: {document.InformalOrderDocumentType.Value}.");
			}
		}

		private EdoInOrderDocumentGroupType MatchDocumentsByGroupType(EdoInOrderDocumentType edoInOrderDocumentType)
		{
			switch(edoInOrderDocumentType)
			{
				case EdoInOrderDocumentType.Upd:
				case EdoInOrderDocumentType.Receipt:
				case EdoInOrderDocumentType.Tender:
					return EdoInOrderDocumentGroupType.Primary;
				case EdoInOrderDocumentType.Withdrawal:
					return EdoInOrderDocumentGroupType.Withdrawal;
				case EdoInOrderDocumentType.SaveCode:
					return EdoInOrderDocumentGroupType.Withdrawal;
				case EdoInOrderDocumentType.Bill:
					return EdoInOrderDocumentGroupType.Bill;
				default:
					throw new NotSupportedException($"Не поддерживается отправка документа: {edoInOrderDocumentType}.");
			}
		}
	}
}
