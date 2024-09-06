using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Report;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Comments;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.BusinessTasks
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Задачи по обзвону",
		Nominative = "Задача по обзвону"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class ClientTask : BusinessTask, ICommentedDocument, IValidatableObject
	{
		public virtual string Title => string.Format(" задача по обзвону : {0}", DeliveryPoint?.ShortAddress);

		public virtual IList<Phone> Phones => DeliveryPoint.Phones;

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		DeliveryPoint deliveryPoint;
		[Display(Name = "Aдрес клиента")]
		public virtual DeliveryPoint DeliveryPoint {
			get => deliveryPoint;
			set => SetField(ref deliveryPoint, value);
		}

		ImportanceDegreeType importanceDegree;
		[Display(Name = "Срочность задачи")]
		public virtual ImportanceDegreeType ImportanceDegree {
			get => importanceDegree;
			set => SetField(ref importanceDegree, value);
		}

		[Display(Name = "Период активности задачи (начало)")]
		public virtual DateTime StartActivePeriod { get => EndActivePeriod.AddDays(-1); set { } }

		int tareReturn;
		[Display(Name = "Количество тары на сдачу")]
		public virtual int TareReturn {
			get => tareReturn;
			set => SetField(ref tareReturn, value);
		}

		private TaskSource? source;
		[Display(Name = "Источник")]
		public virtual TaskSource? Source {
			get => source;
			set => SetField(ref source, value);
		}

		private int? sourceDocumentId;
		[Display(Name = "ID документа")]
		public virtual int? SourceDocumentId {
			get => sourceDocumentId;
			set => SetField(ref sourceDocumentId, value);
		}

		private IList<DocumentComment> comments = new List<DocumentComment>();
		[Display(Name = "Комментарии")]
		public virtual IList<DocumentComment> Comments {
			get => comments;
			set => SetField(ref comments, value);
		}

		GenericObservableList<DocumentComment> observableComments;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DocumentComment> ObservableComments {
			get {
				if(observableComments == null) {
					observableComments = new GenericObservableList<DocumentComment>(Comments);
				}
				return observableComments;
			}
		}

		GenericObservableList<DocumentComment> ICommentedDocument.Comments => ObservableComments;

		public virtual void AddComment(DocumentComment comment)
		{
			if(ObservableComments.Contains(comment)) {
				return;
			}
			ObservableComments.Add(comment);
		}

		public virtual void DeleteLastComment(DocumentComment comment)
		{
			if(!ObservableComments.Contains(comment)) {
				return;
			}
			ObservableComments.Remove(comment);
		}

		public virtual ReportInfo CreateReportInfoByClient(IReportInfoFactory reportInfoFactory)
		{
			return CreateReportInfo(reportInfoFactory, Counterparty.Id);
		}

		public virtual ReportInfo CreateReportInfoByDeliveryPoint(IReportInfoFactory reportInfoFactory)
		{
			return CreateReportInfo(reportInfoFactory, DeliveryPoint.Counterparty.Id, DeliveryPoint.Id);
		}

		private ReportInfo CreateReportInfo(IReportInfoFactory reportInfoFactory, int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = "Акт по бутылям-залогам";
			reportInfo.Identifier = "Client.SummaryBottlesAndDeposits";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "startDate", null },
				{ "endDate", null },
				{ "client_id", counterpartyId},
				{ "delivery_point_id", deliveryPointId}
			};
			return reportInfo;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
				yield return new ValidationResult("Должен быть выбран контрагент", new[] { "Countrerparty" });
		}
	}
}
