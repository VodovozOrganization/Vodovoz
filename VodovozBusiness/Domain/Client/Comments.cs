using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Client
{


	public class Comments : PropertyChangedBase, IDomainObject
	{
		public virtual IUnitOfWork UoW { get; set; }

		#region Свойства

		public virtual int Id { get; set; }

		public static IUnitOfWorkGeneric<Comments> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<Comments>();
			return uow;
		}

		Counterparty counterparty;

		[Required]
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField(ref counterparty, value, () => Counterparty); }
		}

		Employee author;

		[Display(Name = "Автор комментария")]

		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		string text;

		[Display(Name = "Текст")]
		public virtual string Text {
			get { return text; }
			set { SetField(ref text, value, () => Text); }
		}

		CommentsGroups commentsGroups;

		[Display(Name = "Группа")]
		public virtual CommentsGroups CommentsGroups {
			get { return commentsGroups; }
			set { SetField(ref commentsGroups, value, () => CommentsGroups); }
		}

		bool isFixed = true;

		[Display(Name = "Постоянный")]
		public virtual bool IsFixed {
			get { return isFixed; }
			set { SetField(ref isFixed, value, () => IsFixed); }
		}

		DateTime createDate = DateTime.Today;

		[Display(Name = "Дата комментария")]
		public virtual DateTime CreateDate {
			get { return createDate; }
			set { SetField(ref createDate, value, () => CreateDate); }
		}

		DeliveryPoint deliveryPoint;
		[Display(Name = "Точка доставки комментария")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField(ref deliveryPoint, value, () => DeliveryPoint); }
		}

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField(ref order, value, () => Order); }
		}

		CommentsAncorPoint? ancorPoint;

		[Display(Name = "Статус заказа")]
		public virtual CommentsAncorPoint? AncorPoint {
			get { return ancorPoint; }
			set { SetField(ref ancorPoint, value, () => AncorPoint); }
		}

		CommentsType commentsType;

		[Display(Name = "Тип комментария")]
		public virtual CommentsType CommentsType {
			get { return commentsType; }
			set { SetField(ref commentsType, value, () => CommentsType); }
		}

		#endregion


		public Comments()
		{
			//Group = String.Empty;
			//Author = String.Empty;
			Text = String.Empty;
		}


		public virtual bool CanTextEdit {
			get {
				if(CommentsType == CommentsType.Alterable)
					return true;
				return false;
			}
		}
	}


	public enum CommentsType
	{
		[Display(Name = "Встроеный")]
		Embedded,
		[Display(Name = "Шаблон")]
		Template,
		[Display(Name = "Редактируемый")]
		Alterable
	}

	public class CommentsTypeStringType : NHibernate.Type.EnumStringType
	{
		public CommentsTypeStringType() : base(typeof(CommentsType))
		{
		}
	}


	public enum CommentsAncorPoint
	{
		[Display(Name = "Контрагент")]
		Counterparty,
		[Display(Name = "Точка доставки")]
		DeliveryPoint,
		[Display(Name = "Заказ")]
		Order
	}

	public class CommentsAncorPointStringType : NHibernate.Type.EnumStringType
	{
		public CommentsAncorPointStringType() : base(typeof(CommentsAncorPoint))
		{
		}
	}
}
