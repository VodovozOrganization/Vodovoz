using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения ТМЦ",
		Nominative = "документ перемещения ТМЦ")]
	[EntityPermission]
	[HistoryTrace]
	public class MovementDocument : Document, IValidatableObject, ITwoWarhousesBindedDocument
	{
		MovementDocumentType documentType;
		[Display(Name = "Тип документа перемещения")]
		public virtual MovementDocumentType DocumentType
		{
			get => documentType;
			set => SetField(ref documentType, value);
		}

		private DateTime timeStamp;
		public override DateTime TimeStamp
		{
			get => timeStamp;
			set => SetField(ref timeStamp, value);
		}

		private MovementDocumentStatus status;
		[Display(Name = "Статус")]
		public virtual MovementDocumentStatus Status
		{
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		private bool hasDiscrepancy;
		[Display(Name = "Имеет расхождение")]
		public virtual bool HasDiscrepancy
		{
			get => hasDiscrepancy;
			set => SetField(ref hasDiscrepancy, value, () => HasDiscrepancy);
		}

		private MovementWagon movementWagon;
		[Display(Name = "Фура")]
		public virtual MovementWagon MovementWagon
		{
			get => movementWagon;
			set => SetField(ref movementWagon, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => comment;
			set => SetField(ref comment, value);
		}

		#region Send

		private Warehouse fromWarehouse;
		[Display(Name = "Склад отправки")]
		public virtual Warehouse FromWarehouse
		{
			get => fromWarehouse;
			set => SetField(ref fromWarehouse, value);
		}

		private Employee sender;
		[Display(Name = "Отправитель")]
		public virtual Employee Sender
		{
			get => sender;
			set => SetField(ref sender, value, () => Sender);
		}

		private DateTime? sendTime;
		[Display(Name = "Время отправления")]
		public virtual DateTime? SendTime
		{
			get => sendTime;
			set => SetField(ref sendTime, value, () => SendTime);
		}

		#endregion Send

		#region Receive

		private Warehouse toWarehouse;
		[Display(Name = "Склад получения")]
		public virtual Warehouse ToWarehouse
		{
			get => toWarehouse;
			set => SetField(ref toWarehouse, value);
		}

		private Employee receiver;
		[Display(Name = "Получатель")]
		public virtual Employee Receiver
		{
			get => receiver;
			set => SetField(ref receiver, value, () => Receiver);
		}

		private DateTime? receiveTime;
		[Display(Name = "Время получения")]
		public virtual DateTime? ReceiveTime
		{
			get => receiveTime;
			set => SetField(ref receiveTime, value);
		}

		#endregion Receive

		#region Discrepancy

		private Employee discrepancyAccepter;
		[Display(Name = "Кто подтвердил расхождения")]
		public virtual Employee DiscrepancyAccepter
		{
			get => discrepancyAccepter;
			set => SetField(ref discrepancyAccepter, value, () => DiscrepancyAccepter);
		}

		private DateTime? discrepancyAcceptTime;
		[Display(Name = "Время подтверждения расхождений")]
		public virtual DateTime? DiscrepancyAcceptTime
		{
			get => discrepancyAcceptTime;
			set => SetField(ref discrepancyAcceptTime, value, () => DiscrepancyAcceptTime);
		}

		#endregion Discrepancy

		private IList<MovementDocumentItem> items = new List<MovementDocumentItem>();
		[Display(Name = "Строки")]
		public virtual IList<MovementDocumentItem> Items
		{
			get => items;
			set
			{
				SetField(ref items, value);
				observableItems = null;
			}
		}

		private GenericObservableList<MovementDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MovementDocumentItem> ObservableItems
		{
			get
			{
				if(observableItems == null)
				{
					observableItems = new GenericObservableList<MovementDocumentItem>(Items);
					observableItems.PropertyOfElementChanged += (sender, e) =>
					{
						var item = sender as MovementDocumentItem;
						if(item == null)
						{
							return;
						}
						if(e.PropertyName == nameof(item.ReceivedAmount))
						{
							OnPropertyChanged(nameof(CanSend));
							OnPropertyChanged(nameof(CanReceive));
						}
					};
				}
				return observableItems;
			}
		}

		public virtual void SetNomenclaturesOnStock(IUnitOfWork uow, IWarehouseRepository warehouseRepository)
		{
			if(uow == null)
				throw new ArgumentNullException(nameof(uow));
			if(warehouseRepository == null)
				throw new ArgumentNullException(nameof(warehouseRepository));

			if(FromWarehouse == null)
			{
				foreach(var item in items)
					item.AmountOnSource = 99999999;
				return;
			}

			var amountOnStock = warehouseRepository.GetWarehouseNomenclatureStock(uow, FromWarehouse.Id, Items.Select(x => x.Nomenclature.Id));
			foreach(var item in Items)
				item.AmountOnSource = amountOnStock.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id).Stock;
		}

		#region Вычисляемые

		public virtual bool IsDelivered => DeliveredStatuses.Contains(Status);

		public virtual string Title => String.Format("Перемещение ТМЦ №{0} от {1:d}", Id, TimeStamp);

		#endregion Вычисляемые

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0)
			{

				if(DocumentType == MovementDocumentType.InnerTransfer)
				{
					yield return new ValidationResult(
						"Внутреннее перемещение на данный момент запрещено.",
						new[] { this.GetPropertyName(o => o.DocumentType) }
					);
				}

				if(!(validationContext.GetService(typeof(IWarehouseRepository)) is IWarehouseRepository warehouseRepository))
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IWarehouseRepository)}");

				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					var amountOnStock = warehouseRepository.GetWarehouseNomenclatureStock(uow, FromWarehouse.Id, Items.Select(x => x.Nomenclature.Id));
					foreach(var item in Items)
					{
						var stock = amountOnStock.First(x => x.NomenclatureId == item.Nomenclature.Id).Stock;
						if(item.SendedAmount > stock)
						{
							yield return new ValidationResult
									(
										$"Нельзя отгружать больше чем есть на складе {Environment.NewLine} " +
										$"На складе: {item.Nomenclature.Name} {stock} {item.Nomenclature?.Unit?.Name}"
									);
						}
					}
				}
			}

			if(!Items.Any())
				yield return new ValidationResult(String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName(o => o.Items) });

			if(DocumentType == MovementDocumentType.InnerTransfer || DocumentType == MovementDocumentType.Transportation)
			{
				if(FromWarehouse == ToWarehouse)
					yield return new ValidationResult("Склады отправления и получения должны различатся.",
						new[] { this.GetPropertyName(o => o.FromWarehouse), this.GetPropertyName(o => o.ToWarehouse) });
				if(FromWarehouse == null)
					yield return new ValidationResult("Склад отправления должен быть указан.",
						new[] { this.GetPropertyName(o => o.FromWarehouse) });
				if(ToWarehouse == null)
					yield return new ValidationResult("Склад получения должен быть указан.",
						new[] { this.GetPropertyName(o => o.ToWarehouse) });
			}

			if(DocumentType == MovementDocumentType.Transportation)
			{
				if(MovementWagon == null)
					yield return new ValidationResult("Фура не указана.",
						new[] { this.GetPropertyName(o => o.MovementWagon) });
			}

			if(Status == MovementDocumentStatus.New)
			{
				foreach(var item in Items)
				{
					if(item.SendedAmount <= 0)
						yield return new ValidationResult(String.Format("Для номенклатуры <{0}> не указано количество.", item.Nomenclature.Name),
							new[] { this.GetPropertyName(o => o.Items) });
				}
			}

			var needWeightOrVolume = Items
				.Select(item => item.Nomenclature)
				.Where(nomenclature =>
					Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category)
					&& (nomenclature.Weight == default
						|| nomenclature.Length == default
						|| nomenclature.Width == default
						|| nomenclature.Height == default))
				.ToList();
			if(needWeightOrVolume.Any())
			{
				yield return new ValidationResult(
					"Для всех добавленных номенклатур должны быть заполнены вес и объём.\n" +
					"Список номенклатур, в которых не заполнен вес или объём:\n" +
					$"{string.Join("\n", needWeightOrVolume.Select(x => $"({x.Id}) {x.Name}"))}",
					new[] { nameof(Items) });
			}
		}

		#endregion

		#region Функции

		//Можно добавлять товары во всех статусах, так как принимающая сторона может доабвлять пропущенные при отправке номенклатуры
		public virtual bool CanAddItem => true;

		public virtual void AddItem(Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			if(!CanAddItem)
			{
				return;
			}

			var item = new MovementDocumentItem
			{
				Nomenclature = nomenclature,
				SendedAmount = amount,
				AmountOnSource = inStock,
				Document = this
			};

			ObservableItems.Add(item);
		}

		public virtual bool CanDeleteItems => Status == MovementDocumentStatus.New || Status == MovementDocumentStatus.Sended;

		public virtual void DeleteItem(MovementDocumentItem item)
		{
			if(item == null)
			{
				return;
			}

			if(!CanDeleteItems)
			{
				return;
			}

			if(!ObservableItems.Contains(item))
			{
				return;
			}

			ObservableItems.Remove(item);
		}

		public virtual bool CanSend => (Status == MovementDocumentStatus.New || Status == MovementDocumentStatus.Sended)
			&& FromWarehouse != null
			&& ToWarehouse != null
			&& Items.Any();

		public virtual void Send(Employee sender)
		{
			if(sender == null)
			{
				throw new ArgumentNullException(nameof(sender));
			}

			if(!CanSend)
			{
				return;
			}

			Status = MovementDocumentStatus.Sended;
			Sender = sender;
			if(!SendTime.HasValue)
			{
				SendTime = DateTime.Now;
			}

			foreach(var item in Items)
			{
				item.ReceivedAmount = item.SendedAmount;
				item.UpdateWriteoffOperation();
			}
		}

		public virtual bool CanReceive
		{
			get
			{
				//Принятие возможно только в указанных ниже статусах
				var receiveStatuses = new[] { MovementDocumentStatus.Sended, MovementDocumentStatus.Discrepancy, MovementDocumentStatus.Accepted };
				return receiveStatuses.Contains(Status) && FromWarehouse != null && ToWarehouse != null && Items.Any();
			}
		}

		public virtual void Receive(Employee employeeReceiver)
		{
			if(employeeReceiver == null)
			{
				throw new ArgumentNullException(nameof(employeeReceiver));
			}

			if(!CanReceive)
			{
				return;
			}

			//Очищаем информацию о расхождениях так как получение могло быть вызвано повторно из принятого статуса
			ClearDiscrepancyInfo();

			if(HasDeliveryDiscrepancies())
			{
				Status = MovementDocumentStatus.Discrepancy;
				HasDiscrepancy = true;
			}
			else
			{
				Status = MovementDocumentStatus.Accepted;
			}
			if(!ReceiveTime.HasValue)
			{
				ReceiveTime = DateTime.Now;
			}

			foreach(var item in Items)
			{
				item.UpdateIncomeOperation();
			}
		}

		private bool HasDeliveryDiscrepancies()
		{
			if(Status == MovementDocumentStatus.New)
			{
				return false;
			}
			foreach(var item in Items)
			{
				if(item.HasDiscrepancy)
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool CanAcceptDiscrepancy => Status == MovementDocumentStatus.Discrepancy && HasDeliveryDiscrepancies();

		public virtual void AcceptDiscrepancy(Employee employeeDiscrepancyAccepter)
		{
			if(employeeDiscrepancyAccepter == null)
			{
				throw new ArgumentNullException(nameof(employeeDiscrepancyAccepter));
			}

			if(!CanAcceptDiscrepancy)
			{
				return;
			}

			DiscrepancyAccepter = employeeDiscrepancyAccepter;
			if(!DiscrepancyAcceptTime.HasValue)
			{
				DiscrepancyAcceptTime = DateTime.Now;
			}
			HasDiscrepancy = true;

			Status = MovementDocumentStatus.Accepted;
		}

		public virtual void ClearDiscrepancyInfo()
		{
			DiscrepancyAccepter = null;
			DiscrepancyAcceptTime = null;
			HasDiscrepancy = false;
		}

		#endregion

		public static IEnumerable<MovementDocumentStatus> DeliveredStatuses => new[] { MovementDocumentStatus.Discrepancy, MovementDocumentStatus.Accepted };
	}

	public enum MovementDocumentStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Отправлен")]
		Sended,
		[Display(Name = "Расхождение")]
		Discrepancy,
		[Display(Name = "Принят")]
		Accepted
	}

	public class MovementDocumentStatusStringType : NHibernate.Type.EnumStringType
	{
		public MovementDocumentStatusStringType() : base(typeof(MovementDocumentStatus))
		{
		}
	}

	public enum MovementDocumentType
	{
		[Display(Name = "Внутреннее перемещение")]
		InnerTransfer,
		[Display(Name = "Транспортировка")]
		Transportation
	}

	public class MovementDocumentCategoryStringType : NHibernate.Type.EnumStringType
	{
		public MovementDocumentCategoryStringType() : base(typeof(MovementDocumentType))
		{
		}
	}
}

