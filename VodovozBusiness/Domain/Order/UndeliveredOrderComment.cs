using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using Gdk;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject(Gender = GrammaticalGender.Masculine,
				NominativePlural = "комментарии к недовезённому заказу",
				Nominative = "комментарий к недовезённому заказу",
				Prepositional = "комментарии к недовезённому заказу",
				PrepositionalPlural = "комментариях к недовезённому заказу"
			   )
	]
	public class UndeliveredOrderComment : BusinessObjectBase<UndeliveredOrderComment>, IDomainObject, IValidatableObject
	{
		#region Cвойства

		public virtual int Id { get; set; }

		UndeliveredOrder undeliveredOrder;
		[Display(Name = "Недовоз")]
		public virtual UndeliveredOrder UndeliveredOrder {
			get { return undeliveredOrder; }
			set { SetField(ref undeliveredOrder, value, () => UndeliveredOrder); }
		}

		CommentedFields commentedField;
		[Display(Name = "Комментируемое поле")]
		public virtual CommentedFields CommentedField {
			get { return commentedField; }
			set { SetField(ref commentedField, value, () => CommentedField); }
		}

		DateTime commentDate;
		[Display(Name = "Дата и время комментария")]
		public virtual DateTime CommentDate {
			get { return commentDate; }
			set { SetField(ref commentDate, value, () => CommentDate); }
		}

		Employee employee;
		[Display(Name = "Пользователь")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		#endregion

		#region Методы

		/// <summary>
		/// Возврат цветного комментария
		/// </summary>
		/// <returns>Комментарий</returns>
		/// <param name="color">Цвет комментария</param>
		public virtual string GetMarkedUpComment(string color){
			string fullComment = String.Format("<b>{0} {1}:</b> {2} ",
			                                   commentDate.ToString("d MMM, hh:mm:ss"),
			                                   StringWorks.PersonNameWithInitials(Employee.LastName, Employee.Name, Employee.Patronymic),
			                                   Comment
			                                  );
			return String.Format("<span foreground=\"{0}\">{1}</span>", color, fullComment);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(Comment))
				yield return new ValidationResult(
					"Оставьте свой комментарий",
					new[] { this.GetPropertyName(u => u.Comment) }
				);
		}

		#endregion
	}

	public enum CommentedFields
	{
		[Display(Name = "Не выбрано")]
		None,
		[Display(Name = "Дата недовезённого заказа")]
		OldOrderDeliveryDate,
		[Display(Name = "Причина")]
		Reason,
		[Display(Name = "Новый заказ")]
		ActionWithInvoice,
		[Display(Name = "Дата нового заказа")]
		TransferDateTime,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Адрес")]
		Address,
		[Display(Name = "Бутыли")]
		UndeliveredOrderItems,
		[Display(Name = "Интервал недовезённого заказа")]
		OldDeliverySchedule,
		[Display(Name = "Автор недовезённого заказа")]
		OldOrderAuthor,
		[Display(Name = "Водитель")]
		DriverName,
		[Display(Name = "Звонок водителя")]
		DriversCall,
		[Display(Name = "Звонок диспетчера")]
		DispatcherCall,
		[Display(Name = "Зафиксировавший недовоз")]
		Registrator,
		[Display(Name = "Автор недовоза")]
		UndeliveryAuthor,
		[Display(Name = "Виновный")]
		Guilty,
		[Display(Name = "Оштрафованные")]
		FinedPeople,
		[Display(Name = "Статус недовоза")]
		Status,
		[Display(Name = "Статус недовезённого заказа на момент отмены")]
		OldOrderStatus
	}

	public class UndeliveredOrderCommentsCommentedFieldsStringType : NHibernate.Type.EnumStringType
	{
		public UndeliveredOrderCommentsCommentedFieldsStringType() : base(typeof(CommentedFields))
		{
		}
	}
}
