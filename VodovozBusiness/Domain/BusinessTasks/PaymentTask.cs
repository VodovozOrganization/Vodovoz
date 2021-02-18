using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Comments;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.BusinessTasks
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Задачи по платежам",
		Nominative = "Задача по платежам"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class PaymentTask : BusinessTask, ICommentedDocument, IValidatableObject
	{
		public virtual string Title => string.Format($" задача по платежам : {Id}");

		//public virtual IList<Phone> Phones => DeliveryPoint.Phones;

		private Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		private Subdivision subdivision;
		[Display(Name = "Отдел")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private TaskSource? source;
		[Display(Name = "Источник")]
		public virtual TaskSource? Source {
			get => source;
			set => SetField(ref source, value);
		}

		OrderPaymentStatus paymentStatus;
		[Display(Name = "Статус оплаты")]
		public virtual OrderPaymentStatus PaymentStatus {
			get => paymentStatus;
			set => SetField(ref paymentStatus, value);
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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
				yield return new ValidationResult("Должен быть выбран контрагент", new[] { nameof(Counterparty) });

			if(Order == null)
				yield return new ValidationResult("Должен быть заполнен заказ", new[] { nameof(Order) });

			if(Order != null) {
				if(IsTaskComplete && PaymentStatus == OrderPaymentStatus.Paid) {
					if(Order.OrderStatus == OrderStatus.WaitForPayment)
						yield return new ValidationResult("Завершение задачи не возможно, т.к. заказ не переведен в статус Принят! " +
							"Либо подтвердите отгрузку, либо перенесите дату завершения задачи", new[] { nameof(Order) });
				}
			}

			if (AssignedEmployee == null)
				yield return new ValidationResult("Должен быть назначен ответственный", new[] { nameof(AssignedEmployee) });
		}
	}
}
