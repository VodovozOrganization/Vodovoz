using Autofac;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Accusative = "недовезённый заказ",
		AccusativePlural = "недовезённые заказы",
		Genitive = "недовезённого заказа",
		GenitivePlural = "недовезённых заказов",
		Nominative = "недовезённый заказ",
		NominativePlural = "недовезённые заказы",
		Prepositional = "недовезённом заказе",
		PrepositionalPlural = "недовезённых заказах")]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveredOrder : BusinessObjectBase<UndeliveredOrder>, IDomainObject, IValidatableObject
	{
		private UndeliveryStatus _undeliveryStatus;
		private Order _oldOrder;
		private Order _newOrder;
		private DriverCallType _driverCallType;
		private int? _driverCallNr;
		private DateTime? _driverCallTime;
		private DateTime? _dispatcherCallTime;
		private Employee _employeeRegistrator;
		private string _reason;
		private UndeliveryProblemSource _problemSource;
		private Employee _author;
		private DateTime _timeOfCreation;
		private Employee _lastEditor;
		private DateTime _lastEditedTime;
		private DeliverySchedule _newDeliverySchedule;
		private TransferType? _orderTransferType;
		private OrderStatus _oldOrderStatus;
		private Subdivision _inProcessAtDepartment;
		private IList<Fine> _fines = new List<Fine>();
		private GenericObservableList<Fine> _observableFines;
		private IList<GuiltyInUndelivery> _guiltyInUndelivery = new List<GuiltyInUndelivery>();
		private GenericObservableList<GuiltyInUndelivery> _observableGuilty;
		private List<UndeliveryProblemSource> _problemSourceItems;

		private UndeliveryTransferAbsenceReason _undeliveryTransferAbsenceReason;
		private IList<UndeliveredOrderResultComment> _resultComments = new List<UndeliveredOrderResultComment>();
		private GenericObservableList<UndeliveredOrderResultComment> _observableResultComments;
		private UndeliveryDetalization _undeliveryDetalization;
		private UndeliveryStatus? _oldUndeliveryStatus;
		private IList<UndeliveryDiscussion> _undeliveryDiscussions = new List<UndeliveryDiscussion>();
		private GenericObservableList<UndeliveryDiscussion> _observableUndeliveryDiscussions;
		private ISubdivisionSettings _subdivisionSettings => ScopeProvider.Scope.Resolve<ISubdivisionSettings>();
		private IEmployeeRepository _employeeRepository => ScopeProvider.Scope.Resolve<IEmployeeRepository>();

		#region Cвойства

		public virtual int Id { get; set; }

		[Display(Name = "Статус недовоза")]
		public virtual UndeliveryStatus UndeliveryStatus
		{
			get => _undeliveryStatus;
			protected set
			{
				SetField(ref _undeliveryStatus, value);
				if(_oldUndeliveryStatus == null)
				{
					_oldUndeliveryStatus = _undeliveryStatus;
				}
			}
		}

		[Display(Name = "Недовоз")]
		public virtual Order OldOrder
		{
			get => _oldOrder;
			set => SetField(ref _oldOrder, value);
		}

		[Display(Name = "Новый заказ")]
		public virtual Order NewOrder
		{
			get => _newOrder;
			set
			{
				if(SetField(ref _newOrder, value))
				{
					NewDeliverySchedule = value.DeliverySchedule;
				}
			}
		}

		[Display(Name = "Место отзвона водителя")]
		public virtual DriverCallType DriverCallType
		{
			get => _driverCallType;
			set => SetField(ref _driverCallType, value);
		}

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallNr
		{
			get => _driverCallNr;
			set => SetField(ref _driverCallNr, value);
		}

		[Display(Name = "Время звонка водителя")]
		public virtual DateTime? DriverCallTime
		{
			get => _driverCallTime;
			set => SetField(ref _driverCallTime, value);
		}

		[Display(Name = "Звонок диспетчера клиенту")]
		public virtual DateTime? DispatcherCallTime
		{
			get => _dispatcherCallTime;
			set => SetField(ref _dispatcherCallTime, value);
		}

		[Display(Name = "Зарегистрировал недовоз")]
		public virtual Employee EmployeeRegistrator
		{
			get => _employeeRegistrator;
			set => SetField(ref _employeeRegistrator, value);
		}

		[Display(Name = "Причина недовоза")]
		public virtual string Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		[Display(Name = "Источник проблемы")]
		public virtual UndeliveryProblemSource ProblemSource
		{
			get => _problemSource;
			set => SetField(ref _problemSource, value);
		}

		[Display(Name = "Создатель недовоза")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime TimeOfCreation
		{
			get => _timeOfCreation;
			set => SetField(ref _timeOfCreation, value);
		}

		[Display(Name = "Последний редактор")]
		[IgnoreHistoryTrace]
		public virtual Employee LastEditor
		{
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}

		[Display(Name = "Время последнего изменения")]
		[IgnoreHistoryTrace]
		public virtual DateTime LastEditedTime
		{
			get => _lastEditedTime;
			set => SetField(ref _lastEditedTime, value);
		}

		[Display(Name = "Время доставки нового заказа")]
		public virtual DeliverySchedule NewDeliverySchedule
		{
			get => _newDeliverySchedule;
			set => SetField(ref _newDeliverySchedule, value);
		}

		[Display(Name = "Вид переноса")]
		public virtual TransferType? OrderTransferType
		{
			get => _orderTransferType;
			set => SetField(ref _orderTransferType, value);
		}

		[Display(Name = "Статус недовезённого заказа")]
		public virtual OrderStatus OldOrderStatus
		{
			get => _oldOrderStatus;
			set => SetField(ref _oldOrderStatus, value);
		}

		[Display(Name = "В работе у отдела")]
		public virtual Subdivision InProcessAtDepartment
		{
			get => _inProcessAtDepartment;
			set => SetField(ref _inProcessAtDepartment, value);
		}

		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines
		{
			get => _fines;
			set => SetField(ref _fines, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines
		{
			get
			{
				if(_observableFines == null)
				{
					_observableFines = new GenericObservableList<Fine>(_fines);
				}
				return _observableFines;
			}
		}

		[Display(Name = "Ответственные в недовозе")]
		public virtual IList<GuiltyInUndelivery> GuiltyInUndelivery
		{
			get => _guiltyInUndelivery;
			set => SetField(ref _guiltyInUndelivery, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GuiltyInUndelivery> ObservableGuilty
		{
			get
			{
				if(_observableGuilty == null)
				{
					_observableGuilty = new GenericObservableList<GuiltyInUndelivery>(_guiltyInUndelivery);
				}
				return _observableGuilty;
			}
		}

		public virtual IEnumerable<UndeliveryProblemSource> ProblemSourceItems
		{
			get
			{
				if(_problemSourceItems == null)
				{
					_problemSourceItems = UoW.GetAll<UndeliveryProblemSource>().Where(k => !k.IsArchive).ToList();
				}

				if(ProblemSource != null && ProblemSource.IsArchive)
				{
					_problemSourceItems.Add(UoW.GetById<UndeliveryProblemSource>(ProblemSource.Id));
				}

				return _problemSourceItems;
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

		[Display(Name = "Обсуждения")]
		public virtual IList<UndeliveryDiscussion> UndeliveryDiscussions
		{
			get => _undeliveryDiscussions;
			set => SetField(ref _undeliveryDiscussions, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<UndeliveryDiscussion> ObservableUndeliveryDiscussions =>
			_observableUndeliveryDiscussions ?? (_observableUndeliveryDiscussions = new GenericObservableList<UndeliveryDiscussion>(UndeliveryDiscussions));

		#endregion

		#region Вычисляемые свойства

		public virtual string Title => $"Недовоз №{Id} от {TimeOfCreation:d}";

		#endregion

		#region Методы

		public virtual void AddGuilty(GuiltyInUndelivery guilty)
		{
			if(guilty.GuiltySide != GuiltyTypes.None)
			{
				var notUndelivery = ObservableGuilty.FirstOrDefault(g => g.GuiltySide == GuiltyTypes.None);
				if(notUndelivery != null)
				{
					ObservableGuilty.Remove(notUndelivery);
				}
			}
			if(guilty.GuiltySide == GuiltyTypes.None && ObservableGuilty.Any())
			{
				return;
			}

			ObservableGuilty.Add(guilty);
		}

		public virtual void AddAutoCommentByChangeStatus()
		{
			var text = string.Empty;

			if(_oldUndeliveryStatus.HasValue && _oldUndeliveryStatus != UndeliveryStatus && Id > 0)
			{
				text = $"сменил(а) статус недовоза\nс \"{_oldUndeliveryStatus.GetEnumTitle()}\" на \"{UndeliveryStatus.GetEnumTitle()}\"";
			}

			if(string.IsNullOrEmpty(text))
			{
				return;
			}

			AddAutoCommentToOkkDiscussion(UoW, text);
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
		/// Добавление комментария к обсуждению ОКК
		/// </summary>
		/// <param name="uow">UoW</param>		
		/// <param name="text">Текст комментария</param>
		public virtual void AddAutoCommentToOkkDiscussion(IUnitOfWork uow, string text)
		{
			var okkDiscussion = OkkDiscussion ?? CreateOkkDiscussion(uow);

			var comment = new UndeliveryDiscussionComment
			{
				Comment = text,
				Author = _employeeRepository.GetEmployeeForCurrentUser(uow),
				UndeliveryDiscussion = okkDiscussion,
				CreationTime = DateTime.Now
			};

			okkDiscussion.ObservableComments.Add(comment);

			uow.Save(okkDiscussion);
		}

		public virtual UndeliveryDiscussion CreateOkkDiscussion(IUnitOfWork uow)
		{
			var okkSubdivision = uow.GetById<Subdivision>(_subdivisionSettings.GetOkkId());

			var okkDiscussion = new UndeliveryDiscussion
			{
				StartSubdivisionDate = DateTime.Now,
				Status = UndeliveryDiscussionStatus.InProcess,
				Undelivery = this,
				Subdivision = okkSubdivision
			};

			ObservableUndeliveryDiscussions.Add(okkDiscussion);

			return okkDiscussion;
		}

		public virtual UndeliveryDiscussion OkkDiscussion => ObservableUndeliveryDiscussions.FirstOrDefault(x => x.Subdivision.Id == _subdivisionSettings.GetOkkId());

		/// <summary>
		/// Сбор различной информации о недоставленном заказе
		/// </summary>
		/// <returns>Строка</returns>
		public virtual string GetOldOrderInfo(IOrderRepository orderRepository)
		{
			StringBuilder info = new StringBuilder("\n").AppendLine($"<b>Автор недовоза:</b> {Author?.ShortName}");
			if(_oldOrder != null)
			{
				info.AppendLine($"<b>Автор накладной:</b> {WebUtility.HtmlEncode(_oldOrder.Author?.ShortName)}");
				info.AppendLine($"<b>Клиент:</b> {WebUtility.HtmlEncode(_oldOrder.Client.Name)}");
				if(_oldOrder.SelfDelivery)
				{
					info.AppendLine("<b>Адрес:</b> \"Самовывоз\"");
				}
				else
				{
					info.AppendLine($"<b>Адрес:</b> {WebUtility.HtmlEncode(_oldOrder.DeliveryPoint?.ShortAddress)}");
				}

				info.AppendLine($"<b>Дата заказа:</b> {_oldOrder.DeliveryDate.Value:dd.MM.yyyy}");
				if(_oldOrder.SelfDelivery || _oldOrder.DeliverySchedule == null)
				{
					info.AppendLine("<b>Интервал:</b> \"Самовывоз\"");
				}
				else
				{
					info.AppendLine($"<b>Интервал:</b> {WebUtility.HtmlEncode(_oldOrder.DeliverySchedule.Name)}");
				}

				info.AppendLine($"<b>Сумма отменённого заказа:</b> {CurrencyWorks.GetShortCurrencyString(_oldOrder.OrderSum)}");
				int watter19LQty = orderRepository.Get19LWatterQtyForOrder(UoW, _oldOrder);
				var eqToClient = orderRepository.GetEquipmentToClientForOrder(UoW, _oldOrder);
				var eqFromClient = orderRepository.GetEquipmentFromClientForOrder(UoW, _oldOrder);

				if(watter19LQty > 0)
				{
					info.AppendLine($"<b>19л вода:</b> {watter19LQty}");
				}
				else if(eqToClient.Any())
				{
					string eq = string.Empty;
					foreach(var e in eqToClient)
					{
						eq += $"{e.ShortName ?? e.Name} - {e.Count}, ";
					}

					info.AppendLine($"<b>К клиенту:</b> {eq.Trim(new char[] { ' ', ',' })}");
				}
				else if(eqFromClient.Any())
				{
					string eq = string.Empty;
					foreach(var e in eqFromClient)
					{
						eq += $"{e.ShortName ?? e.Name} - {e.Count}\n";
					}

					info.AppendLine($"<b>От клиента:</b> {eq.Trim()}");
				}

				var drivers = GetDrivers(orderRepository);
				if(drivers.Any())
				{
					var sb = new StringBuilder();
					foreach(var d in drivers)
					{
						sb.Append($"{WebUtility.HtmlEncode(d.ShortName)} ← ");
					}

					info.AppendLine($"<b>Водитель:</b> {sb.ToString().Trim(new char[] { ' ', '←' })}");
				}
				var routeLists = orderRepository.GetAllRLForOrder(UoW, OldOrder);
				if(routeLists.Any())
				{
					StringBuilder rls = new StringBuilder();

					foreach(var l in routeLists)
					{
						rls.Append($"{l.Id} ← ");
					}

					info.AppendLine($"<b>Маршрутный лист:</b> {rls.ToString().Trim(new char[] { ' ', '←' })}");
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
			{
				info.AppendLine($"<i>В работе у отдела:</i> {InProcessAtDepartment.Name}");
			}

			if(ObservableGuilty.Any())
			{
				info.AppendLine("<i>Ответственные:</i> ");
				foreach(GuiltyInUndelivery g in ObservableGuilty)
				{
					info.AppendLine($"\t{g}");
				}
			}
			var routeLists = orderRepository.GetAllRLForOrder(UoW, OldOrder);
			if(routeLists.Any())
			{
				info.AppendLine($"<i>Место:</i> {DriverCallType.GetEnumTitle()}");
				if(DriverCallTime.HasValue)
				{
					info.AppendLine($"<i>Время звонка водителя:</i> {DriverCallTime.Value:HH:mm}");
				}
			}
			if(DriverCallTime.HasValue)
			{
				info.AppendLine($"<i>Время звонка клиенту:</i> {DispatcherCallTime.Value:HH:mm}");
			}

			if(NewOrder != null)
			{
				info.AppendLine($"<i>Перенос:</i> {NewOrder.Title}, {NewOrder.DeliverySchedule?.DeliveryTime ?? "инт-л не выбран"}");
			}

			info.AppendLine($"<i>Причина:</i> {Reason}");
			return info.ToString();
		}

		public virtual void AttachSubdivisionToDiscussions(Subdivision subdivision)
		{
			if(subdivision == null)
			{
				throw new ArgumentNullException(nameof(subdivision));
			}

			if(ObservableUndeliveryDiscussions.Any(x => x.Subdivision.Id == subdivision.Id))
			{
				return;
			}

			UndeliveryDiscussion newDiscussion = new UndeliveryDiscussion
			{
				StartSubdivisionDate = DateTime.Now,
				PlannedCompletionDate = DateTime.Today,
				Undelivery = this,
				Subdivision = subdivision
			};

			ObservableUndeliveryDiscussions.Add(newDiscussion);

			SetStatus(UndeliveryStatus.InProcess);
		}

		public virtual void UpdateUndeliveryStatusByDiscussionsStatus()
		{
			if(ObservableUndeliveryDiscussions.All(x => x.Status == UndeliveryDiscussionStatus.Closed))
			{
				SetStatus(UndeliveryStatus.Checking);
				return;
			}

			SetStatus(UndeliveryStatus.InProcess);
		}

		public virtual IList<string> SetStatus(UndeliveryStatus newStatus)
		{
			List<string> result = new List<string>();
			if(newStatus == UndeliveryStatus.Closed)
			{
				if(ObservableResultComments.Count == 0)
				{
					result.Add("Необходимо добавить комментарий \"Результат\".");
				}
			}

			if(!result.Any())
			{
				UndeliveryStatus = newStatus;
			}

			return result;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var orderRepository = validationContext.GetRequiredService<IOrderRepository>();

			if(OldOrder == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать недовезённый заказ",
					new[] { nameof(OldOrder) });
			}

			if(OldOrder != null && NewOrder != null && OldOrder.Id == NewOrder.Id)
			{
				yield return new ValidationResult(
					"Перенесённый заказ не может совпадать с недовезённым",
					new[] { nameof(OldOrder), nameof(NewOrder) });
			}

			if(NewOrder != null && OrderTransferType == null)
			{
				yield return new ValidationResult("Необходимо указать тип переноса");
			}

			if(string.IsNullOrWhiteSpace(Reason))
			{
				yield return new ValidationResult(
					"Не заполнено поле \"Что случилось?\"",
					new[] { nameof(Reason) });
			}

			if(!ObservableGuilty.Any())
			{
				yield return new ValidationResult(
					"Необходимо выбрать Ответственого",
					new[] { nameof(ObservableGuilty) });
			}

			if(InProcessAtDepartment == null)
			{
				yield return new ValidationResult(
					"Необходимо заполнить поле \"В работе у отдела\"",
					new[] { nameof(InProcessAtDepartment) });
			}

			if(ObservableGuilty.Count() > 1 && ObservableGuilty.Any(g => g.GuiltySide == GuiltyTypes.None))
			{
				yield return new ValidationResult(
					"Определитесь, кто ответственный! Либо это не недовоз, либо кто-то ответственный!",
					new[] { nameof(GuiltyInUndelivery) });
			}

			if(ObservableGuilty.Any(g => g.GuiltySide == GuiltyTypes.Department && g.GuiltyDepartment == null))
			{
				yield return new ValidationResult(
					"Не выбран отдел в одном или нескольких Ответственных.",
					new[] { nameof(GuiltyInUndelivery) });
			}

			if(EmployeeRegistrator == null)
			{
				yield return new ValidationResult("Не указан сотрудник, зарегистрировавший недовоз.",
					new[] { nameof(EmployeeRegistrator) });
			}

			#region Статусы для отмены

			var isOrderStatusForbiddenForCancellation = !orderRepository.GetStatusesForOrderCancelationWithCancellation().Contains(OldOrder.OrderStatus);
			var isSelfDeliveryOnLoadingOrder = NewOrder != null && NewOrder.SelfDelivery && OldOrder.OrderStatus == OrderStatus.OnLoading;
			var isNotUndelivery = GuiltyInUndelivery.Any(x => x.GuiltySide == GuiltyTypes.None);

			if(Id == 0 && isOrderStatusForbiddenForCancellation && !isSelfDeliveryOnLoadingOrder && !isNotUndelivery)
			{
				yield return new ValidationResult("В текущий момент заказ нельзя отменить",
					new[] { nameof(OrderStatus) });
			}

			if(Id > 0 && isOrderStatusForbiddenForCancellation && !isSelfDeliveryOnLoadingOrder && !isNotUndelivery)
			{
				yield return new ValidationResult(
					"Чтобы изменить недовоз, выберите ответственного Нет не недовоз, т.к исходный заказ закрыт",
					new[] { nameof(OrderStatus) });
			}

			#endregion
		}

		#endregion
	}
}
