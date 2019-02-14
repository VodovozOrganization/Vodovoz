using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repositories;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
				NominativePlural = "недовезённые заказы",
				Nominative = "недовезённый заказ",
				Prepositional = "недовезённом заказе",
				PrepositionalPlural = "недовезённых заказах"
			   )
	]
	[HistoryTrace]
	public class UndeliveredOrder : BusinessObjectBase<UndeliveredOrder>, IDomainObject, IValidatableObject
	{
		#region Cвойства

		public virtual int Id { get; set; }

		UndeliveryStatus undeliveryStatus;

		[Display(Name = "Статус недовоза")]
		public virtual UndeliveryStatus UndeliveryStatus {
			get => undeliveryStatus;
			protected set { SetField(ref undeliveryStatus, value, () => UndeliveryStatus); }
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

		Subdivision inProcessAtDepartment;

		[Display(Name = "В работе у отдела")]
		public virtual Subdivision InProcessAtDepartment {
			get { return inProcessAtDepartment; }
			set { SetField(ref inProcessAtDepartment, value, () => InProcessAtDepartment); }
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

		IList<GuiltyInUndelivery> guiltyInUndelivery = new List<GuiltyInUndelivery>();
		[Display(Name = "Виновные в недовозе")]
		public virtual IList<GuiltyInUndelivery> GuiltyInUndelivery {
			get { return guiltyInUndelivery; }
			set { SetField(ref guiltyInUndelivery, value, () => GuiltyInUndelivery); }
		}

		GenericObservableList<GuiltyInUndelivery> observableGuilty;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GuiltyInUndelivery> ObservableGuilty {
			get {
				if(observableGuilty == null) {
					observableGuilty = new GenericObservableList<GuiltyInUndelivery>(guiltyInUndelivery);
				}
				return observableGuilty;
			}
		}

		UndeliveryStatus? InitialStatus { get; set; } = null;

		#endregion

		#region Вычисляемые свойства

		public virtual string Title => String.Format("Недовоз №{0} от {1:d}", Id, TimeOfCreation);

		#endregion

		#region Методы

		/// <summary>
		/// Смена статуса недовоза
		/// </summary>
		/// <param name="status">Status.</param>
		public virtual void SetUndeliveryStatus(UndeliveryStatus status)
		{
			InitialStatus = UndeliveryStatus;
			UndeliveryStatus = status;
			AddAutoComment(CommentedFields.Reason);
		}

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

		public virtual void Close(){
			SetUndeliveryStatus(UndeliveryStatus.Closed);
			LastEditor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			LastEditedTime = DateTime.Now;
		}

		/// <summary>
		/// Добавление автокомментариев к полям
		/// </summary>
		/// <param name="field">Комментируемое поле</param>
		void AddAutoComment(CommentedFields field)
		{
			string text = String.Empty;
			switch(field) {
				case CommentedFields.Reason:
					if(InitialStatus != null && InitialStatus != UndeliveryStatus && Id > 0)
						text = String.Format(
							"сменил(а) статус недовоза\nс \"{0}\" на \"{1}\"",
							InitialStatus.GetEnumTitle(),
							UndeliveryStatus.GetEnumTitle()
						);
					break;
				default:
					break;
			}
			if(String.IsNullOrEmpty(text))
				return;

			AddCommentToTheField(UoW, field, text);
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

		/// <summary>
		/// Сбор различной информации о недоставленном заказе
		/// </summary>
		/// <returns>Строка</returns>
		public virtual string GetOldOrderInfo()
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
				info.AppendLine(String.Format("<b>Сумма отменённого заказа:</b> {0}", CurrencyWorks.GetShortCurrencyString(oldOrder.TotalSum)));
				int watter19LQty = OrderRepository.Get19LWatterQtyForOrder(UoW, oldOrder);
				var eqToClient = OrderRepository.GetEquipmentToClientForOrder(UoW, oldOrder);
				var eqFromClient = OrderRepository.GetEquipmentFromClientForOrder(UoW, oldOrder);

				if(watter19LQty > 0) {
					info.AppendLine(String.Format("<b>19л вода:</b> {0}", watter19LQty));
				} else if(eqToClient.Any()) {
					string eq = String.Empty;
					foreach(var e in eqToClient)
						eq += String.Format("{0} - {1}, ", e.ShortName ?? e.Name, e.Count);
					info.AppendLine(String.Format("<b>К клиенту:</b> {0}", eq.Trim(new Char[] { ' ', ',' })));
				} else if(eqFromClient.Any()) {
					string eq = String.Empty;
					foreach(var e in eqFromClient) 
						eq += String.Format("{0} - {1}\n", e.ShortName ?? e.Name, e.Count);
					info.AppendLine(String.Format("<b>От клиента:</b> {0}", eq.Trim()));
				}
				if(GetDrivers().Any()) {
					StringBuilder drivers = new StringBuilder();
					foreach(var d in GetDrivers())
						drivers.AppendFormat("{0} ← ", d.ShortName);
					info.AppendLine(String.Format("<b>Водитель:</b> {0}", drivers.ToString().Trim(new char[] { ' ', '←' })));
				}
				var routeLists = OrderRepository.GetAllRLForOrder(UoW, OldOrder);
				if(routeLists.Any()) {
					StringBuilder rls = new StringBuilder();
					foreach(var l in routeLists)
						rls.AppendFormat("{0} ← ", l.Id);
					info.AppendLine(String.Format("<b>Маршрутный лист:</b> {0}", rls.ToString().Trim(new char[] { ' ', '←' })));
				}
			}

			return info.ToString();
		}

		/// <summary>
		/// Получение полей недовоза в виде строки
		/// </summary>
		/// <returns>Строка</returns>
		public virtual string GetUndeliveryInfo()
		{
			StringBuilder info = new StringBuilder("\n");
			if(InProcessAtDepartment != null)
				info.AppendLine(String.Format("<i>В работе у отдела:</i> {0}", InProcessAtDepartment.Name));
			if(ObservableGuilty.Any()) {
				info.AppendLine("<i>Виновные:</i> ");
				foreach(GuiltyInUndelivery g in ObservableGuilty)
					info.AppendLine(String.Format("\t{0}", g));
			}
			var routeLists = OrderRepository.GetAllRLForOrder(UoW, OldOrder);
			if(routeLists.Any()) {
				info.AppendLine(String.Format("<i>Место:</i> {0}", DriverCallType.GetEnumTitle()));
				if(DriverCallTime.HasValue)
					info.AppendLine(String.Format("<i>Время звонка водителя:</i> {0}", DriverCallTime.Value.ToString("HH:mm")));
			}
			if(DriverCallTime.HasValue)
				info.AppendLine(String.Format("<i>Время звонка клиенту:</i> {0}", DispatcherCallTime.Value.ToString("HH:mm")));
			if(NewOrder != null)
				info.AppendLine(String.Format("<i>Перенос:</i> {0}, {1}", NewOrder.Title, NewOrder.DeliverySchedule?.DeliveryTime ?? "инт-л не выбран"));
			info.AppendLine(String.Format("<i>Причина:</i> {0}", Reason));
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

			if(!ObservableGuilty.Any())
				yield return new ValidationResult(
					"Необходимо выбрать виновного",
					new[] { this.GetPropertyName(u => u.ObservableGuilty) }
				);

			if(InProcessAtDepartment == null)
				yield return new ValidationResult(
					"Необходимо заполнить поле \"В работе у отдела\"",
					new[] { this.GetPropertyName(u => u.InProcessAtDepartment) }
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
}
