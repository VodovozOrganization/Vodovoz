using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NHibernate.Util;
using QSHistoryLog;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repositories;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
				NominativePlural = "недовезённые заказы",
				Nominative = "недовезённый заказ",
				Prepositional = "недовезённом заказе",
				PrepositionalPlural = "недовезённых заказах"
			   )
	]
	public class UndeliveredOrder : BusinessObjectBase<UndeliveredOrder>, IDomainObject, IValidatableObject
	{
		#region Cвойства

		public virtual int Id { get; set; }

		UndeliveryStatus undeliveryStatus;

		[Display(Name = "Статус недовоза")]
		public virtual UndeliveryStatus UndeliveryStatus {
			get { return undeliveryStatus; }
			set { SetField(ref undeliveryStatus, value, () => UndeliveryStatus); }
		}

		Order oldOrder;

		[Display(Name = "Недовоз")]
		public virtual Order OldOrder {
			get { return oldOrder; }
			set { SetField(ref oldOrder, value, () => OldOrder); }
		}

		Order newOrder;

		[Display(Name = "Новый заказ")]
		public virtual Order NewOrder {
			get { return newOrder; }
			set {
				if(SetField(ref newOrder, value, () => NewOrder))
					NewDeliverySchedule = value.DeliverySchedule;
			}
		}

		GuiltyTypes? guiltySide;

		[Display(Name = "Виновная сторона")]
		public virtual GuiltyTypes? GuiltySide {
			get { return guiltySide; }
			set { SetField(ref guiltySide, value, () => GuiltySide); }
		}

		Subdivision guiltyDepartment;

		[Display(Name = "Виновный отдел ВВ")]
		public virtual Subdivision GuiltyDepartment {
			get { return guiltyDepartment; }
			set { SetField(ref guiltyDepartment, value, () => GuiltyDepartment); }
		}

		DriverCallType driverCallType;

		[Display(Name = "Место отзвона водителя")]
		public virtual DriverCallType DriverCallType {
			get { return driverCallType; }
			set { SetField(ref driverCallType, value, () => DriverCallType); }
		}

		int? driverCallNr;

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallNr {
			get { return driverCallNr; }
			set { SetField(ref driverCallNr, value, () => DriverCallNr); }
		}

		DateTime? driverCallTime;
		[Display(Name = "Время звонка водителя")]
		public virtual DateTime? DriverCallTime {
			get { return driverCallTime; }
			set { SetField(ref driverCallTime, value, () => DriverCallTime); }
		}

		DateTime? dispatcherCallTime;
		[Display(Name = "Звонок диспетчера клиенту")]
		public virtual DateTime? DispatcherCallTime {
			get { return dispatcherCallTime; }
			set { SetField(ref dispatcherCallTime, value, () => DispatcherCallTime); }
		}

		Employee employeeRegistrator;

		[Display(Name = "Зарегистрировал недовоз")]
		public virtual Employee EmployeeRegistrator {
			get { return employeeRegistrator; }
			set { SetField(ref employeeRegistrator, value, () => EmployeeRegistrator); }
		}

		string reason;

		[Display(Name = "Причина недовоза")]
		public virtual string Reason {
			get { return reason; }
			set { SetField(ref reason, value, () => Reason); }
		}

		Employee author;

		[Display(Name = "Создатель недовоза")]
		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		DateTime timeOfCreation;

		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime TimeOfCreation {
			get { return timeOfCreation; }
			set { SetField(ref timeOfCreation, value, () => TimeOfCreation); }
		}

		Employee lastEditor;

		[Display(Name = "Последний редактор")]
		[IgnoreHistoryTrace]
		public virtual Employee LastEditor {
			get { return lastEditor; }
			set { SetField(ref lastEditor, value, () => LastEditor); }
		}

		DateTime lastEditedTime;

		[Display(Name = "Время последнего изменения")]
		[IgnoreHistoryTrace]
		public virtual DateTime LastEditedTime {
			get { return lastEditedTime; }
			set { SetField(ref lastEditedTime, value, () => LastEditedTime); }
		}

		DeliverySchedule newDeliverySchedule;

		[Display(Name = "Время доставки нового заказа")]
		public virtual DeliverySchedule NewDeliverySchedule {
			get { return newDeliverySchedule; }
			set { SetField(ref newDeliverySchedule, value, () => NewDeliverySchedule); }
		}

		OrderStatus oldOrderStatus;

		[Display(Name = "Статус недовезённого заказа")]
		public virtual OrderStatus OldOrderStatus {
			get { return oldOrderStatus; }
			set { SetField(ref oldOrderStatus, value, () => OldOrderStatus); }
		}

		IList<Fine> fines = new List<Fine>();
		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines {
			get { return fines; }
			set { SetField(ref fines, value, () => Fines); }
		}

		GenericObservableList<Fine> observableFines;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines {
			get {
				if(observableFines == null) {
					observableFines = new GenericObservableList<Fine>(fines);
				}
				return observableFines;
			}
		}

		#endregion

		#region Вычисляемые свойства

		public virtual string Title => String.Format("Недовоз №{0} от {1:d}", Id, TimeOfCreation);

		public virtual IList<Employee> UndeliveredOrderDrivers {
			get {
				var routeListItem = RouteListItemRepository.GetRouteListItemForOrder(UoW, OldOrder);
				//UndeliveredOrderDrivers.Add(routeListItem.RouteList.Driver);

				//FIX добавить водителей, если были переносы заказа

				return UndeliveredOrderDrivers;
			}
		}

		#endregion

		#region Методы

		public virtual IList<Employee> GetDrivers()
		{
			var rls = OrderRepository.GetAllRLForOrder(UoW, OldOrder);
			return rls?.Select(r => r.Driver).ToList();
		}

		public virtual string GetAllCommentsForTheField(CommentedFields field)
		{
			var comments = UndeliveredOrderCommentsRepository.GetComments(UoW, this, field);
			StringBuilder sb = new StringBuilder();

			int cnt = 0;
			foreach(var comment in comments) {
				sb.AppendLine(comment.GetMarkedUpComment(cnt++ % 2 == 0 ? "red" : "blue"));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Добавление комментария к полю
		/// </summary>
		/// <param name="uow">UoW</param>
		/// <param name="field">Комментируемое поле</param>
		/// <param name="text">Текст комментария</param>
		public virtual void AddCommentToTheField(IUnitOfWork uow, CommentedFields field, string text)
		{
			UndeliveredOrderComment comment = new UndeliveredOrderComment {
				Comment = text,
				CommentDate = DateTime.Now,
				CommentedField = field,
				Employee = EmployeeRepository.GetEmployeeForCurrentUser(uow),
				UndeliveredOrder = this
			};

			uow.Save(comment);
		}

		public virtual string GetUndeliveryInfo()
		{
			StringBuilder info = new StringBuilder("\n").AppendLine(String.Format("<b>Автор недовоза:</b> {0}", Author.ShortName));
			if(oldOrder != null) {
				info.AppendLine(String.Format("<b>Автор накладной:</b> {0}", oldOrder.Author?.ShortName));
				info.AppendLine(String.Format("<b>Клиент:</b> {0}", oldOrder.Client.Name));
				if(oldOrder.SelfDelivery)
					info.AppendLine(String.Format("<b>Адрес:</b> {0}", "Самовывоз"));
				else
					info.AppendLine(String.Format("<b>Адрес:</b> {0}", oldOrder.DeliveryPoint?.ShortAddress));
				info.AppendLine(String.Format("<b>Дата заказа:</b> {0}", oldOrder.DeliveryDate.Value.ToString("dd.MM.yyyy")));
				if(oldOrder.SelfDelivery || oldOrder.DeliverySchedule == null)
					info.AppendLine(String.Format("<b>Интервал:</b> {0}", "Самовывоз"));
				else
					info.AppendLine(String.Format("<b>Интервал:</b> {0}", oldOrder.DeliverySchedule.Name));

				int watter19LQty = OrderRepository.Get19LWatterQtyForOrder(UoW, oldOrder);
				var eqToClient = OrderRepository.GetEquipmentToClientForOrder(UoW, oldOrder);
				var eqFromClient = OrderRepository.GetEquipmentFromClientForOrder(UoW, oldOrder);

				if(watter19LQty > 0) {
					info.AppendLine(String.Format("<b>19л вода:</b> {0}", watter19LQty));
				} else if(eqToClient.Any()) {
					string eq = String.Empty;
					eqToClient.ForEach(e => eq += String.Format("{0} - {1}, ", e.ShortName ?? e.Name, e.Count));
					info.AppendLine(String.Format("<b>К клиенту:</b> {0}", eq.Trim(new Char[] { ' ', ',' })));
				} else if(eqFromClient.Any()) {
					string eq = String.Empty;
					eqFromClient.ForEach(e => eq += String.Format("{0} - {1}\n", e.ShortName ?? e.Name, e.Count));
					info.AppendLine(String.Format("<b>От клиента:</b> {0}", eq.Trim()));
				}
				if(GetDrivers().Any()) {
					StringBuilder drivers = new StringBuilder();
					GetDrivers().ForEach(d => drivers.AppendFormat("{0} ← ", d.ShortName));
					info.AppendLine(String.Format("<b>Водитель:</b> {0}", drivers.ToString().Trim(new char[] { ' ', '←' })));
				}
				var routeLists = OrderRepository.GetAllRLForOrder(UoW, OldOrder);
				if(routeLists.Any()) {
					StringBuilder rls = new StringBuilder();
					routeLists.ForEach(l => rls.AppendFormat("{0} ← ", l.Id));
					info.AppendLine(String.Format("<b>Маршрутный лист:</b> {0}", rls.ToString().Trim(new char[] { ' ', '←' })));
				}
			}

			return info.ToString();
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(OldOrder == null)
				yield return new ValidationResult(
					"Необходимо выбрать недовезённый заказ",
					new[] { this.GetPropertyName(u => u.OldOrder) }
				);

			if(OldOrder != null && NewOrder != null && OldOrder.Id == NewOrder.Id)
				yield return new ValidationResult(
					"Перенесённый заказ не может совпадать с недовезённым",
					new[] { this.GetPropertyName(u => u.OldOrder), this.GetPropertyName(u => u.NewOrder) }
				);

			if(String.IsNullOrWhiteSpace(Reason))
				yield return new ValidationResult(
					"Не заполнено поле \"Причина\"",
					new[] { this.GetPropertyName(u => u.Reason) }
				);

			if(GuiltySide == null)
				yield return new ValidationResult(
					"Необходимо выбрать виновного",
					new[] { this.GetPropertyName(u => u.GuiltySide) }
				);
		}

		#endregion
	}

	public enum UndeliveryStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "На проверке")]
		Checking,
		[Display(Name = "Закрыт")]
		Closed
	}

	public class UndeliveredOrderUndeliveryStatusStringType : NHibernate.Type.EnumStringType
	{
		public UndeliveredOrderUndeliveryStatusStringType() : base(typeof(UndeliveryStatus))
		{
		}
	}

	public enum GuiltyTypes
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Водитель")]
		Driver,
		[Display(Name = "Отдел ВВ")]
		Department,
		[Display(Name = "Нет (не недовоз)")]
		None
	}

	public class UndeliveredOrderGuiltySideStringType : NHibernate.Type.EnumStringType
	{
		public UndeliveredOrderGuiltySideStringType() : base(typeof(GuiltyTypes))
		{
		}
	}
}
