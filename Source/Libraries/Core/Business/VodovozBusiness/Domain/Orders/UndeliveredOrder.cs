using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;

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
		private UndeliveryTransferAbsenceReason _undeliveryTransferAbsenceReason;
		private IList<UndeliveredOrderResultComment> _resultComments = new List<UndeliveredOrderResultComment>();
		private GenericObservableList<UndeliveredOrderResultComment> _observableResultComments;
		private UndeliveryDetalization _undeliveryDetalization;
		private UndeliveryStatus? _oldUndeliveryStatus;
		private UndeliveryStatus _undeliveryStatus;

		#region Cвойства

		public virtual int Id { get; set; }


		[Display(Name = "Статус недовоза")]
		public virtual UndeliveryStatus UndeliveryStatus
		{
			get => _undeliveryStatus;
			protected set
			{
				_oldUndeliveryStatus = _undeliveryStatus;
				SetField(ref _undeliveryStatus, value);
			}
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

		UndeliveryProblemSource problemSource;
		[Display(Name = "Источник проблемы")]
		public virtual UndeliveryProblemSource ProblemSource {
			get { return problemSource; }
			set { SetField(ref problemSource, value, () => ProblemSource); }
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

		TransferType? orderTransferType;
		[Display(Name = "Вид переноса")]
		public virtual TransferType? OrderTransferType {
			get { return orderTransferType; }
			set { SetField(ref orderTransferType, value, () => OrderTransferType); }
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
		[Display(Name = "Ответственные в недовозе")]
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

		List<UndeliveryProblemSource> problemSourceItems;

		public virtual IEnumerable<UndeliveryProblemSource> ProblemSourceItems {
			get {
				if(problemSourceItems == null)
					problemSourceItems = UoW.GetAll<UndeliveryProblemSource>().Where(k => !k.IsArchive).ToList();
				if(ProblemSource != null && ProblemSource.IsArchive)
					problemSourceItems.Add(UoW.GetById<UndeliveryProblemSource>(ProblemSource.Id));

				return problemSourceItems;
			}
		}

		[Display(Name = "Причина отсутствия переноса")]
		public virtual UndeliveryTransferAbsenceReason UndeliveryTransferAbsenceReason
		{
			get => _undeliveryTransferAbsenceReason;
			set => SetField(ref _undeliveryTransferAbsenceReason, value);
		}

		[Display(Name = "Комментарии - результаты")]
		public virtual IList<UndeliveredOrderResultComment> ResultComments
		{
			get => _resultComments;
			set => SetField(ref _resultComments, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<UndeliveredOrderResultComment> ObservableResultComments
		{
			get
			{
				if(_observableResultComments == null)
				{
					_observableResultComments = new GenericObservableList<UndeliveredOrderResultComment>(ResultComments);
				}

				return _observableResultComments;
			}
		}

		[Display(Name = "Детализация")]
		public virtual UndeliveryDetalization UndeliveryDetalization
		{
			get => _undeliveryDetalization;
			set => SetField(ref _undeliveryDetalization, value);
		}

		#endregion

		#region Вычисляемые свойства

		public virtual string Title => string.Format("Недовоз №{0} от {1:d}", Id, TimeOfCreation);

		#endregion

		#region Методы

		public virtual void AddGuilty(GuiltyInUndelivery guilty)
		{
			if(guilty.GuiltySide != GuiltyTypes.None) {
				var notUndelivery = ObservableGuilty.FirstOrDefault(g => g.GuiltySide == GuiltyTypes.None);
				if(notUndelivery != null)
					ObservableGuilty.Remove(notUndelivery);
			}
			if(guilty.GuiltySide == GuiltyTypes.None && ObservableGuilty.Any())
				return;

			ObservableGuilty.Add(guilty);
		}

		public virtual void AddAutoCommentByChangeStatus()
		{
			AddAutoComment(CommentedFields.Reason);
		}

		public virtual IList<Employee> GetDrivers(IOrderRepository orderRepository)
		{
			var rls = orderRepository.GetAllRLForOrder(UoW, OldOrder);
			return rls?.Select(r => r.Driver).ToList();
		}
		
		public virtual void Close(Employee currentEmployee)
		{
			UndeliveryStatus = UndeliveryStatus.Closed;
			AddAutoCommentByChangeStatus();
			LastEditor = currentEmployee;
			LastEditedTime = DateTime.Now;
		}

		/// <summary>
		/// Добавление автокомментариев к полям
		/// </summary>
		/// <param name="field">Комментируемое поле</param>
		void AddAutoComment(CommentedFields field)
		{
			var text = string.Empty;
			switch(field) {
				case CommentedFields.Reason:
					if(_oldUndeliveryStatus.HasValue && _oldUndeliveryStatus != UndeliveryStatus && Id > 0)
					{
						text =
							$"сменил(а) статус недовоза\nс \"{_oldUndeliveryStatus.GetEnumTitle()}\" на \"{UndeliveryStatus.GetEnumTitle()}\"";
					}

					break;
				default:
					break;
			}
			if(string.IsNullOrEmpty(text))
			{
				return;
			}

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
				Employee = new EmployeeRepository().GetEmployeeForCurrentUser(uow),
				UndeliveredOrder = this
			};

			uow.Save(comment);
		}

		/// <summary>
		/// Сбор различной информации о недоставленном заказе
		/// </summary>
		/// <returns>Строка</returns>
		public virtual string GetOldOrderInfo(IOrderRepository orderRepository)
		{
			StringBuilder info = new StringBuilder("\n").AppendLine(string.Format("<b>Автор недовоза:</b> {0}", Author.ShortName));
			if(oldOrder != null) {
				info.AppendLine(string.Format("<b>Автор накладной:</b> {0}", oldOrder.Author?.ShortName));
				info.AppendLine(string.Format("<b>Клиент:</b> {0}", oldOrder.Client.Name));
				if(oldOrder.SelfDelivery)
					info.AppendLine(string.Format("<b>Адрес:</b> {0}", "Самовывоз"));
				else
					info.AppendLine(string.Format("<b>Адрес:</b> {0}", oldOrder.DeliveryPoint?.ShortAddress));
				info.AppendLine(string.Format("<b>Дата заказа:</b> {0}", oldOrder.DeliveryDate.Value.ToString("dd.MM.yyyy")));
				if(oldOrder.SelfDelivery || oldOrder.DeliverySchedule == null)
					info.AppendLine(string.Format("<b>Интервал:</b> {0}", "Самовывоз"));
				else
					info.AppendLine(string.Format("<b>Интервал:</b> {0}", oldOrder.DeliverySchedule.Name));
				info.AppendLine(string.Format("<b>Сумма отменённого заказа:</b> {0}", CurrencyWorks.GetShortCurrencyString(oldOrder.OrderSum)));
				int watter19LQty = orderRepository.Get19LWatterQtyForOrder(UoW, oldOrder);
				var eqToClient = orderRepository.GetEquipmentToClientForOrder(UoW, oldOrder);
				var eqFromClient = orderRepository.GetEquipmentFromClientForOrder(UoW, oldOrder);

				if(watter19LQty > 0) {
					info.AppendLine(string.Format("<b>19л вода:</b> {0}", watter19LQty));
				} else if(eqToClient.Any()) {
					string eq = string.Empty;
					foreach(var e in eqToClient)
						eq += string.Format("{0} - {1}, ", e.ShortName ?? e.Name, e.Count);
					info.AppendLine(string.Format("<b>К клиенту:</b> {0}", eq.Trim(new char[] { ' ', ',' })));
				} else if(eqFromClient.Any()) {
					string eq = string.Empty;
					foreach(var e in eqFromClient)
						eq += string.Format("{0} - {1}\n", e.ShortName ?? e.Name, e.Count);
					info.AppendLine(string.Format("<b>От клиента:</b> {0}", eq.Trim()));
				}

				var drivers = GetDrivers(orderRepository);
				if(drivers.Any())
				{
					var sb = new StringBuilder();
					foreach(var d in drivers)
					{
						sb.AppendFormat("{0} ← ", d.ShortName);
					}

					info.AppendLine(string.Format("<b>Водитель:</b> {0}", sb.ToString().Trim(new char[] { ' ', '←' })));
				}
				var routeLists = orderRepository.GetAllRLForOrder(UoW, OldOrder);
				if(routeLists.Any()) {
					StringBuilder rls = new StringBuilder();
					foreach(var l in routeLists)
						rls.AppendFormat("{0} ← ", l.Id);
					info.AppendLine(string.Format("<b>Маршрутный лист:</b> {0}", rls.ToString().Trim(new char[] { ' ', '←' })));
				}
			}

			return info.ToString();
		}

		/// <summary>
		/// Получение полей недовоза в виде строки
		/// </summary>
		/// <returns>Строка</returns>
		public virtual string GetUndeliveryInfo(IOrderRepository orderRepository)
		{
			StringBuilder info = new StringBuilder("\n");
			if(InProcessAtDepartment != null)
				info.AppendLine(string.Format("<i>В работе у отдела:</i> {0}", InProcessAtDepartment.Name));
			if(ObservableGuilty.Any()) {
				info.AppendLine("<i>Ответственные:</i> ");
				foreach(GuiltyInUndelivery g in ObservableGuilty)
					info.AppendLine(string.Format("\t{0}", g));
			}
			var routeLists = orderRepository.GetAllRLForOrder(UoW, OldOrder);
			if(routeLists.Any()) {
				info.AppendLine(string.Format("<i>Место:</i> {0}", DriverCallType.GetEnumTitle()));
				if(DriverCallTime.HasValue)
					info.AppendLine(string.Format("<i>Время звонка водителя:</i> {0}", DriverCallTime.Value.ToString("HH:mm")));
			}
			if(DriverCallTime.HasValue)
				info.AppendLine(string.Format("<i>Время звонка клиенту:</i> {0}", DispatcherCallTime.Value.ToString("HH:mm")));
			if(NewOrder != null)
				info.AppendLine(string.Format("<i>Перенос:</i> {0}, {1}", NewOrder.Title, NewOrder.DeliverySchedule?.DeliveryTime ?? "инт-л не выбран"));
			info.AppendLine(string.Format("<i>Причина:</i> {0}", Reason));
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

			if(NewOrder != null && OrderTransferType == null)
				yield return new ValidationResult("Необходимо указать тип переноса");

			if(string.IsNullOrWhiteSpace(Reason))
				yield return new ValidationResult(
					"Не заполнено поле \"Что случилось?\"",
					new[] { this.GetPropertyName(u => u.Reason) }
				);

			if(!ObservableGuilty.Any())
				yield return new ValidationResult(
					"Необходимо выбрать Ответственого",
					new[] { this.GetPropertyName(u => u.ObservableGuilty) }
				);

			if(InProcessAtDepartment == null)
				yield return new ValidationResult(
					"Необходимо заполнить поле \"В работе у отдела\"",
					new[] { this.GetPropertyName(u => u.InProcessAtDepartment) }
				);

			if(ObservableGuilty.Count() > 1 && ObservableGuilty.Any(g => g.GuiltySide == GuiltyTypes.None))
				yield return new ValidationResult(
					"Определитесь, кто ответственный! Либо это не недовоз, либо кто-то ответственный!",
					new[] { this.GetPropertyName(u => u.GuiltyInUndelivery) }
				);

			if(ObservableGuilty.Any(g => g.GuiltySide == GuiltyTypes.Department && g.GuiltyDepartment == null))
				yield return new ValidationResult(
					"Не выбран отдел в одном или нескольких Ответственных.",
					new[] { this.GetPropertyName(u => u.GuiltyInUndelivery) }
				);

			if(EmployeeRegistrator == null)
			{
				yield return new ValidationResult("Не указан сотрудник, зарегистрировавший недовоз.",
					new[] { nameof(EmployeeRegistrator) });
			}
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

	public enum TransferType
	{
		[Display(Name = "Автоперенос согласован")]
		AutoTransferApproved,
		[Display(Name = "Автоперенос н/согл")]
		AutoTransferNotApproved,
		[Display(Name = "Перенос клиентом")]
		TransferredByCounterparty
	}

	public class UndeliveredOrderTransferTypeStringType : NHibernate.Type.EnumStringType
	{
		public UndeliveredOrderTransferTypeStringType() : base(typeof(TransferType))
		{
		}
	}

	public class UndeliveredOrderUndeliveryStatusStringType : NHibernate.Type.EnumStringType
	{
		public UndeliveredOrderUndeliveryStatusStringType() : base(typeof(UndeliveryStatus))
		{
		}
	}
}
