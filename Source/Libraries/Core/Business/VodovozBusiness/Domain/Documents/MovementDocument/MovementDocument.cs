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
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения ТМЦ",
		Nominative = "документ перемещения ТМЦ")]
	[EntityPermission]
	[HistoryTrace]
	public class MovementDocument : Document, IValidatableObject
	{
		private MovementDocumentType _documentType;
		private DateTime timeStamp;
		private MovementDocumentStatus _status;
		private bool _hasDiscrepancy;
		private MovementWagon _movementWagon;
		private string _comment;
		private Warehouse _fromWarehouse;
		private Employee _fromEmployee;
		private Car _fromCar;
		private Employee _sender;
		private DateTime? _sendTime;
		private Warehouse _toWarehouse;
		private Employee _toEmployee;
		private Car _toCar;
		private Employee _receiver;
		private DateTime? _receiveTime;
		private Employee _discrepancyAccepter;
		private DateTime? _discrepancyAcceptTime;
		private MovementDocumentTypeByStorage _movementDocumentTypeByStorage;
		private Storage _storageFrom;
		private IList<MovementDocumentItem> _items = new List<MovementDocumentItem>();
		private GenericObservableList<MovementDocumentItem> _observableItems;
		
		public MovementDocument()
		{
			MovementDocumentTypeByStorage = MovementDocumentTypeByStorage.ForWarehouse;
			StorageFrom = Storage.Warehouse;
		}

		[Display(Name = "Тип документа перемещения")]
		public virtual MovementDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		public override DateTime TimeStamp
		{
			get => timeStamp;
			set => SetField(ref timeStamp, value);
		}

		[Display(Name = "Статус")]
		public virtual MovementDocumentStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Имеет расхождение")]
		public virtual bool HasDiscrepancy
		{
			get => _hasDiscrepancy;
			set => SetField(ref _hasDiscrepancy, value);
		}

		[Display(Name = "Фура")]
		public virtual MovementWagon MovementWagon
		{
			get => _movementWagon;
			set => SetField(ref _movementWagon, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		#region Send

		[Display(Name = "Склад отправки")]
		public virtual Warehouse FromWarehouse
		{
			get => _fromWarehouse;
			set
			{
				if(SetField(ref _fromWarehouse, value))
				{
					if(value is null)
					{
						return;
					}
					
					FromEmployee = null;
					FromCar = null;
				}
			}
		}
		
		[Display(Name = "Сотрудник, с которого делают перемещение")]
		public virtual Employee FromEmployee
		{
			get => _fromEmployee;
			set
			{
				if(SetField(ref _fromEmployee, value))
				{
					if(value is null)
					{
						return;
					}
					
					FromWarehouse = null;
					FromCar = null;
				}
			}
		}
		
		[Display(Name = "Автомобиль, с которого делают перемещение")]
		public virtual Car FromCar
		{
			get => _fromCar;
			set
			{
				if(SetField(ref _fromCar, value))
				{
					if(value is null)
					{
						return;
					}

					FromWarehouse = null;
					FromEmployee = null;
				}
			}
		}

		[Display(Name = "Отправитель")]
		public virtual Employee Sender
		{
			get => _sender;
			set => SetField(ref _sender, value);
		}

		[Display(Name = "Время отправления")]
		public virtual DateTime? SendTime
		{
			get => _sendTime;
			set => SetField(ref _sendTime, value);
		}

		#endregion Send

		#region Receive

		[Display(Name = "Склад получения")]
		public virtual Warehouse ToWarehouse
		{
			get => _toWarehouse;
			set
			{
				if(SetField(ref _toWarehouse, value))
				{
					if(value is null)
					{
						return;
					}

					ToEmployee = null;
					ToCar = null;
				}
			}
		}

		[Display(Name = "Сотрудник, на которого делают перемещение")]
		public virtual Employee ToEmployee
		{
			get => _toEmployee;
			set
			{
				if(SetField(ref _toEmployee, value))
				{
					if(value is null)
					{
						return;
					}
					
					ToWarehouse = null;
					ToCar = null;
				}
			}
		}
		
		[Display(Name = "Автомобиль, на который делают перемещение")]
		public virtual Car ToCar
		{
			get => _toCar;
			set
			{
				if(SetField(ref _toCar, value))
				{
					if(value is null)
					{
						return;
					}

					ToWarehouse = null;
					ToEmployee = null;
				}
			}
		}

		[Display(Name = "Получатель")]
		public virtual Employee Receiver
		{
			get => _receiver;
			set => SetField(ref _receiver, value);
		}

		[Display(Name = "Время получения")]
		public virtual DateTime? ReceiveTime {
			get => _receiveTime;
			set => SetField(ref _receiveTime, value);
		}

		#endregion Receive

		#region Discrepancy

		[Display(Name = "Кто подтвердил расхождения")]
		public virtual Employee DiscrepancyAccepter
		{
			get => _discrepancyAccepter;
			set => SetField(ref _discrepancyAccepter, value);
		}

		[Display(Name = "Время подтверждения расхождений")]
		public virtual DateTime? DiscrepancyAcceptTime
		{
			get => _discrepancyAcceptTime;
			set => SetField(ref _discrepancyAcceptTime, value);
		}

		#endregion Discrepancy

		public virtual MovementDocumentTypeByStorage MovementDocumentTypeByStorage
		{
			get => _movementDocumentTypeByStorage;
			set => SetField(ref _movementDocumentTypeByStorage, value);
		}
		
		public virtual Storage StorageFrom
		{
			get => _storageFrom;
			set => SetField(ref _storageFrom, value);
		}
		
		[Display(Name = "Строки")]
		public virtual IList<MovementDocumentItem> Items
		{
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MovementDocumentItem> ObservableItems
		{
			get
			{
				if(_observableItems == null)
				{
					_observableItems = new GenericObservableList<MovementDocumentItem>(Items);
					_observableItems.PropertyOfElementChanged += (sender, e) =>
					{
						if(!(sender is MovementDocumentItem item))
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
				return _observableItems;
			}
		}

		public virtual void SetNomenclaturesOnStock(IUnitOfWork uow, IWarehouseRepository warehouseRepository)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(warehouseRepository == null)
			{
				throw new ArgumentNullException(nameof(warehouseRepository));
			}

			if(FromWarehouse == null) 
			{
				foreach(var item in _items)
				{
					item.AmountOnSource = 99999999;
				}

				return;
			}

			var amountOnStock = warehouseRepository.GetWarehouseNomenclatureStock(uow, FromWarehouse.Id ,Items.Select(x => x.Nomenclature.Id));
			foreach(var item in Items)
			{
				item.AmountOnSource = amountOnStock.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id).Stock;
			}
		}

		#region Вычисляемые

		public virtual bool IsDelivered => DeliveredStatuses.Contains(Status);

		public virtual string Title => String.Format("Перемещение ТМЦ №{0} от {1:d}", Id, TimeStamp);

		#endregion Вычисляемые

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id == 0) {

				if(DocumentType == MovementDocumentType.InnerTransfer) {
					yield return new ValidationResult(
						"Внутреннее перемещение на данный момент запрещено.",
						new[] { this.GetPropertyName(o => o.DocumentType) }
					);
				}

				if(!(validationContext.GetService(typeof(IWarehouseRepository)) is IWarehouseRepository warehouseRepository))
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IWarehouseRepository)}");

				//TODO обновить проверку количества под новые условия 
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var amountOnStock = warehouseRepository.GetWarehouseNomenclatureStock(uow, FromWarehouse.Id, Items.Select(x => x.Nomenclature.Id));
					foreach(var item in Items) {
						var stock = amountOnStock.First(x => x.NomenclatureId == item.Nomenclature.Id).Stock;
						if(item.SentAmount > stock) {
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

			//TODO Также обновить условие
			if(DocumentType == MovementDocumentType.InnerTransfer || DocumentType == MovementDocumentType.Transportation) {
				if(FromWarehouse == ToWarehouse)
					yield return new ValidationResult("Склады отправления и получения должны различаться.",
						new[] { this.GetPropertyName(o => o.FromWarehouse), this.GetPropertyName(o => o.ToWarehouse) });
				if(FromWarehouse == null)
					yield return new ValidationResult("Склад отправления должен быть указан.",
						new[] { this.GetPropertyName(o => o.FromWarehouse) });
				if(ToWarehouse == null)
					yield return new ValidationResult("Склад получения должен быть указан.",
						new[] { this.GetPropertyName(o => o.ToWarehouse) });
			}

			if(DocumentType == MovementDocumentType.Transportation) {
				if(MovementWagon == null)
					yield return new ValidationResult("Фура не указана.",
						new[] { this.GetPropertyName(o => o.MovementWagon) });
			}

			if(Status == MovementDocumentStatus.New) {
				foreach(var item in Items) {
					if(item.SentAmount <= 0)
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

			var item = CreateBulkItem();
			item.Nomenclature = nomenclature;
			item.SentAmount = amount;
			item.AmountOnSource = inStock;
			item.Document = this;

			ObservableItems.Add(item);
		}

		public virtual void AddItem(InventoryNomenclatureInstance instance, decimal amount, decimal inStock)
		{
			if(!CanAddItem)
			{
				return;
			}

			var item = CreateInstanceItem();
			item.InventoryNomenclatureInstance = instance;
			item.SentAmount = amount;
			item.AmountOnSource = inStock;
			item.Document = this;

			ObservableItems.Add(item);
		}

		public virtual bool CanDeleteItems => Status == MovementDocumentStatus.New || Status == MovementDocumentStatus.Sended;

		public virtual void DeleteItem(MovementDocumentItem item)
		{
			if(item == null) {
				return;
			}

			if(!CanDeleteItems) {
				return;
			}

			if(!ObservableItems.Contains(item)) {
				return;
			}

			ObservableItems.Remove(item);
		}

		public virtual bool CanSend => (Status == MovementDocumentStatus.New || Status == MovementDocumentStatus.Sended)
			&& CanSendByDocumentTypeAndStorage()
			&& Items.Any();

		public virtual void Send(Employee sender)
		{
			if(sender == null) {
				throw new ArgumentNullException(nameof(sender));
			}

			if(!CanSend) {
				return;
			}

			Status = MovementDocumentStatus.Sended;
			Sender = sender;
			if(!SendTime.HasValue) {
				SendTime = DateTime.Now;
			}

			foreach(var item in Items) {
				item.ReceivedAmount = item.SentAmount;
				item.UpdateWriteOffOperation();
			}
		}

		public virtual bool CanReceive {
			get {
				//Принятие возможно только в указанных ниже статусах
				var receiveStatuses = new[] { MovementDocumentStatus.Sended, MovementDocumentStatus.Discrepancy, MovementDocumentStatus.Accepted };
				return receiveStatuses.Contains(Status) && FromWarehouse != null && ToWarehouse != null && Items.Any();
			}
		}

		public virtual void Receive(Employee employeeReceiver)
		{
			if(employeeReceiver == null) {
				throw new ArgumentNullException(nameof(employeeReceiver));
			}

			if(!CanReceive) {
				return;
			}

			//Очищаем информацию о расхождениях так как получение могло быть вызвано повторно из принятого статуса
			ClearDiscrepancyInfo();

			if(HasDeliveryDiscrepancies()) {
				Status = MovementDocumentStatus.Discrepancy;
				HasDiscrepancy = true;
			} else {
				Status = MovementDocumentStatus.Accepted;
			}
			if(!ReceiveTime.HasValue) {
				ReceiveTime = DateTime.Now;
			}

			foreach(var item in Items) {
				item.UpdateIncomeOperation();
			}
		}

		public virtual bool CanAcceptDiscrepancy => Status == MovementDocumentStatus.Discrepancy && HasDeliveryDiscrepancies();

		public virtual void AcceptDiscrepancy(Employee employeeDiscrepancyAccepter)
		{
			if(employeeDiscrepancyAccepter == null) {
				throw new ArgumentNullException(nameof(employeeDiscrepancyAccepter));
			}

			if(!CanAcceptDiscrepancy) {
				return;
			}

			DiscrepancyAccepter = employeeDiscrepancyAccepter;
			if(!DiscrepancyAcceptTime.HasValue) {
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
		
		private bool CanSendByDocumentTypeAndStorage()
		{
			switch(MovementDocumentTypeByStorage)
			{
				case MovementDocumentTypeByStorage.ForWarehouse:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return FromWarehouse != null && ToWarehouse != null;
						case Storage.Employee:
							return FromEmployee != null && ToWarehouse != null;
						case Storage.Car:
							return FromCar != null && ToWarehouse != null;
						default:
							return false;
					}
				case MovementDocumentTypeByStorage.ForEmployee:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return FromWarehouse != null && ToEmployee != null;
						case Storage.Employee:
							return FromEmployee != null && ToEmployee != null;
						case Storage.Car:
							return FromCar != null && ToEmployee != null;
						default:
							return false;
					}
				case MovementDocumentTypeByStorage.ForCar:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return FromWarehouse != null && ToCar != null;
						case Storage.Employee:
							return FromEmployee != null && ToCar != null;
						case Storage.Car:
							return FromCar != null && ToCar != null;
						default:
							return false;
					}
				default:
					return false;
			}
		}
		
		private MovementDocumentItem CreateBulkItem()
		{
			switch(MovementDocumentTypeByStorage)
			{
				case MovementDocumentTypeByStorage.ForWarehouse:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new BulkMovementDocumentFromToWarehouseItem();
						case Storage.Employee:
							return new BulkMovementDocumentFromEmployeeToWarehouseItem();
						case Storage.Car:
							return new BulkMovementDocumentFromCarToWarehouseItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				case MovementDocumentTypeByStorage.ForEmployee:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new BulkMovementDocumentFromWarehouseToEmployeeItem();
						case Storage.Employee:
							return new BulkMovementDocumentFromToEmployeeItem();
						case Storage.Car:
							return new BulkMovementDocumentFromCarToEmployeeItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				case MovementDocumentTypeByStorage.ForCar:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new BulkMovementDocumentFromWarehouseToCarItem();
						case Storage.Employee:
							return new BulkMovementDocumentFromEmployeeToCarItem();
						case Storage.Car:
							return new BulkMovementDocumentFromToCarItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private InstanceMovementDocumentItem CreateInstanceItem()
		{
			switch(MovementDocumentTypeByStorage)
			{
				case MovementDocumentTypeByStorage.ForWarehouse:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new InstanceMovementDocumentFromToWarehouseItem();
						case Storage.Employee:
							return new InstanceMovementDocumentFromEmployeeToWarehouseItem();
						case Storage.Car:
							return new InstanceMovementDocumentFromCarToWarehouseItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				case MovementDocumentTypeByStorage.ForEmployee:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new InstanceMovementDocumentFromWarehouseToEmployeeItem();
						case Storage.Employee:
							return new InstanceMovementDocumentFromToEmployeeItem();
						case Storage.Car:
							return new InstanceMovementDocumentFromCarToEmployeeItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				case MovementDocumentTypeByStorage.ForCar:
					switch(StorageFrom)
					{
						case Storage.Warehouse:
							return new InstanceMovementDocumentFromCarToWarehouseItem();
						case Storage.Employee:
							return new InstanceMovementDocumentFromCarToEmployeeItem();
						case Storage.Car:
							return new InstanceMovementDocumentFromToCarItem();
						default:
							throw new ArgumentOutOfRangeException();
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		public static IEnumerable<MovementDocumentStatus> DeliveredStatuses =>
			new[] { MovementDocumentStatus.Discrepancy, MovementDocumentStatus.Accepted }; 
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

	public enum MovementDocumentTypeByStorage
	{
		[Display(Name = "Для склада")]
		ForWarehouse,
		[Display(Name = "Для сотрудника")]
		ForEmployee,
		[Display(Name = "Для автомобиля")]
		ForCar
	}
	
	public enum Storage
	{
		[Display(Name = "Cклад")]
		Warehouse,
		[Display(Name = "Cотрудник")]
		Employee,
		[Display(Name = "Aвтомобиль")]
		Car
	}

	public class MovementDocumentCategoryStringType : NHibernate.Type.EnumStringType
	{
		public MovementDocumentCategoryStringType() : base(typeof(MovementDocumentType))
		{
		}
	}
}

