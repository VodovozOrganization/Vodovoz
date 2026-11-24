using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Репозиторий для получения истории кода маркировки в ДВ.
	/// </summary>
	public interface ITrueMarkCodeHistoryRepository
	{
		/// <summary>
		/// Собирает историю по коду маркировки.
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="gtin">GTIN кода. Может быть не заполнен, если код введён не полностью.</param>
		/// <param name="serialNumber">Серийный номер кода.</param>
		/// <param name="rawCode">Транспортный код маркировки. Используется, если не заполнены GTIN и серийный номер.</param>
		/// <returns>Данные истории кода маркировки.</returns>
		TrueMarkCodeHistoryData GetCodeHistory(
			IUnitOfWork uow,
			string gtin,
			string serialNumber,
			string rawCode = null);
	}

	/// <summary>
	/// Данные истории кода маркировки в ДВ.
	/// </summary>
	public sealed class TrueMarkCodeHistoryData
	{
		/// <summary>
		/// Экземплярные коды, найденные по искомому коду.
		/// </summary>
		public IList<TrueMarkCodeHistoryCode> Codes { get; set; } = new List<TrueMarkCodeHistoryCode>();

		/// <summary>
		/// Агрегации, в которые входит код.
		/// </summary>
		public IList<TrueMarkCodeHistoryAggregation> Aggregations { get; set; } = new List<TrueMarkCodeHistoryAggregation>();

		/// <summary>
		/// Коды товаров ЧЗ, связанные с найденными экземплярными кодами.
		/// </summary>
		public IList<TrueMarkCodeHistoryProductCode> ProductCodes { get; set; } = new List<TrueMarkCodeHistoryProductCode>();

		/// <summary>
		/// Задачи ЭДО, связанные с кодами товаров.
		/// </summary>
		public IList<TrueMarkCodeHistoryTask> Tasks { get; set; } = new List<TrueMarkCodeHistoryTask>();

		/// <summary>
		/// Заказы, связанные с кодами товаров.
		/// </summary>
		public IList<TrueMarkCodeHistoryOrder> Orders { get; set; } = new List<TrueMarkCodeHistoryOrder>();

		/// <summary>
		/// Документооборот, связанный с кодами товаров.
		/// </summary>
		public IList<TrueMarkCodeHistoryDocumentFlow> DocumentFlows { get; set; } = new List<TrueMarkCodeHistoryDocumentFlow>();
	}

	/// <summary>
	/// Экземплярный код маркировки.
	/// </summary>
	public sealed class TrueMarkCodeHistoryCode
	{
		public int Id { get; set; }
		public string Gtin { get; set; }
		public string SerialNumber { get; set; }
		public string RawCode { get; set; }
		public bool IsInvalid { get; set; }
		public int? ParentWaterGroupCodeId { get; set; }
		public int? ParentTransportCodeId { get; set; }

		/// <summary>
		/// Код идентификации экземпляра.
		/// </summary>
		public string IdentificationCode => $"01{Gtin}21{SerialNumber}";
	}

	/// <summary>
	/// Агрегация (групповой или транспортный код), в которую входит экземплярный код.
	/// </summary>
	public sealed class TrueMarkCodeHistoryAggregation
	{
		public int Id { get; set; }
		public string Code { get; set; }

		/// <summary>
		/// Отображаемый тип агрегации.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// GTIN группового кода. Для транспортного кода не заполняется.
		/// </summary>
		public string Gtin { get; set; }

		/// <summary>
		/// Серийный номер группового кода. Для транспортного кода не заполняется.
		/// </summary>
		public string SerialNumber { get; set; }

		/// <summary>
		/// Признак невалидного кода.
		/// </summary>
		public bool IsInvalid { get; set; }

		/// <summary>
		/// Идентификатор родительской агрегации.
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Тип родительской агрегации.
		/// </summary>
		public string ParentType { get; set; }

		/// <summary>
		/// Идентификатор экземплярного кода, для цепочки которого построена агрегация.
		/// </summary>
		public int CodeId { get; set; }
	}

	/// <summary>
	/// Код товара ЧЗ (TrueMarkProductCode).
	/// </summary>
	public sealed class TrueMarkCodeHistoryProductCode
	{
		public int Id { get; set; }

		/// <summary>
		/// Отображаемый тип кода товара.
		/// </summary>
		public string Type { get; set; }

		public SourceProductCodeStatus Status { get; set; }
		public ProductCodeProblem Problem { get; set; }
		public string SourceCode { get; set; }
		public string ResultCode { get; set; }
		public DateTime? LastModified { get; set; }

		public int? OrderId { get; set; }
		public int? TaskId { get; set; }
		public int? CustomerEdoRequestId { get; set; }
		public DateTime? CustomerEdoRequestTime { get; set; }

		/// <summary>
		/// Тип документа, в котором был отсканирован/добавлен код.
		/// </summary>
		public TrueMarkCodeHistoryProductCodeDocumentType SourceDocumentType { get; set; }

		/// <summary>
		/// Идентификатор документа, в котором был отсканирован/добавлен код.
		/// </summary>
		public int? SourceDocumentId { get; set; }
	}

	/// <summary>
	/// Тип документа, в котором используется код товара.
	/// </summary>
	public enum TrueMarkCodeHistoryProductCodeDocumentType
	{
		None,
		RouteList,
		CarLoadDocument,
		SelfDeliveryDocument
	}

	/// <summary>
	/// Задача ЭДО, связанная с кодом маркировки.
	/// </summary>
	public sealed class TrueMarkCodeHistoryTask
	{
		public int Id { get; set; }
		public EdoTaskType Type { get; set; }
		public EdoTaskStatus Status { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public int? OrderId { get; set; }
	}

	/// <summary>
	/// Заказ, связанный с кодом маркировки.
	/// </summary>
	public sealed class TrueMarkCodeHistoryOrder
	{
		public int Id { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
	}

	/// <summary>
	/// Документооборот, связанный с кодом маркировки.
	/// </summary>
	public sealed class TrueMarkCodeHistoryDocumentFlow
	{
		public int OutgoingDocumentId { get; set; }
		public int? OrderId { get; set; }
		public int? OrderTaskId { get; set; }
		public EdoType EdoType { get; set; }
		public EdoDocumentType DocumentType { get; set; }
		public EdoDocumentStatus Status { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime? SendTime { get; set; }
		public DateTime? AcceptTime { get; set; }
		public EdoDiscrepancyState DiscrepancyState { get; set; }

		public Guid? TaxcomDocflowId { get; set; }
		public EdoDocFlowStatus? TaxcomDocflowStatus { get; set; }
		public DateTime? TaxcomStateChangeTime { get; set; }
		public bool? IsReceived { get; set; }
		public string ErrorMessage { get; set; }
	}
}
