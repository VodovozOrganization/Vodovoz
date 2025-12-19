using Autofac;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Domain.Documents
{
	/// <summary>
	/// Отпуск самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "отпуски самовывоза",
		Nominative = "отпуск самовывоза")]
	[EntityPermission]
	[HistoryTrace]
	public class SelfDeliveryDocument : SelfDeliveryDocumentEntity, IValidatableObject, IWarehouseBoundedDocument
	{
		private Order _order;
		private string _comment;
		private IList<SelfDeliveryDocumentItem> _items
			= new List<SelfDeliveryDocumentItem>();
		private GenericObservableList<SelfDeliveryDocumentItem> _observableItems;
		private IList<SelfDeliveryDocumentReturned> _returnedItems
			= new List<SelfDeliveryDocumentReturned>();
		private int _defBottleId;
		private int _returnedTareBefore;
		private int _tareToReturn;

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;

				if(!NHibernate.NHibernateUtil.IsInitialized(Items))
				{
					return;
				}

				foreach(var item in Items)
				{
					if(item.GoodsAccountingOperation != null
						&& item.GoodsAccountingOperation.OperationTime != TimeStamp)
					{
						item.GoodsAccountingOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		/// <summary>
		/// Заказ, по которому оформляется самовывоз
		/// </summary>
		[Required(ErrorMessage = "Заказ должен быть указан.")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Комментарий к самовывозу
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Строки самовывоза
		/// </summary>
		[Display(Name = "Строки")]
		public virtual new IList<SelfDeliveryDocumentItem> Items
		{
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		/// <summary>
		/// Строки самовывоза
		/// </summary>
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SelfDeliveryDocumentItem> ObservableItems
		{
			get
			{
				if(_observableItems == null)
				{
					_observableItems = new GenericObservableList<SelfDeliveryDocumentItem>(Items);
				}

				return _observableItems;
			}
		}

		/// <summary>
		/// Строки возврата
		/// </summary>
		[Display(Name = "Строки возврата")]
		public virtual IList<SelfDeliveryDocumentReturned> ReturnedItems
		{
			get => _returnedItems;
			set => SetField(ref _returnedItems, value);
		}

		#region Не сохраняемые

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public virtual string Title => $"Самовывоз №{Id} от {TimeStamp:d}";

		/// <summary>
		/// Количество возвратов, которые были оформлены до оформления текущего самовывоза
		/// </summary>
		[PropertyChangedAlso("ReturnedTareBeforeText")]
		public virtual int ReturnedTareBefore
		{
			get => _returnedTareBefore;
			set => SetField(ref _returnedTareBefore, value);
		}

		/// <summary>
		/// Текст для отображения количества возвратов, которые были оформлены до оформления текущего самовывоза
		/// </summary>
		public virtual string ReturnedTareBeforeText =>
			ReturnedTareBefore > 0
			? $"Возвращено другими самовывозами: {ReturnedTareBefore} бут."
			: string.Empty;

		/// <summary>
		/// Количество тары, которую нужно вернуть
		/// </summary>
		public virtual int TareToReturn
		{
			get => _tareToReturn;
			set => SetField(ref _tareToReturn, value);
		}

		#endregion

		/// <summary>
		/// Проверка валидности документа самовывоза
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.Items
					.TryGetValue("skipTrueMarkCodesCheck", out var value)
				&& value is bool skipTrueMarkCodesCheck))
			{
				skipTrueMarkCodesCheck = false;
			}

			if(!(validationContext.GetService(typeof(IUnitOfWork)) is IUnitOfWork unitOfWork))
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			if(!(validationContext.GetService(typeof(ICommonServices)) is ICommonServices commonServices))
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			foreach(var item in Items)
			{
				if(item.Amount > item.AmountInStock)
				{
					yield return new ValidationResult(
						$"На складе недостаточное количество <{item.Nomenclature.Name}>",
						new[] { this.GetPropertyName(o => o.Items) });
				}

				if(item.Amount <= 0)
				{
					yield return new ValidationResult(
						$"Введено не положительное количество <{item.Nomenclature.Name}>",
						new[] { this.GetPropertyName(o => o.Items) });
				}

				var count =  decimal.ToInt32(item.Document.GetNomenclaturesCountInOrder(item.Nomenclature));
				if(item.Amount != count)
				{
					yield return new ValidationResult(
						$"Нельзя частично отгрузить номенклатуру <{item.Nomenclature.Name}> в заказе. Для отпуска необходимо {count} шт., а не {decimal.ToInt32(item.Amount)} шт.",
						new[] { this.GetPropertyName(o => o.Items) });
				}
				
				if(!skipTrueMarkCodesCheck
					&& !commonServices.CurrentPermissionService.ValidatePresetPermission(
					   Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteListItem.CanSetCompletedStatusWhenNotAllTrueMarkCodesAdded)
				   && Order.Client.ReasonForLeaving == ReasonForLeaving.Resale
				   && item.Nomenclature.IsAccountableInTrueMark
				   && item.Amount > item.TrueMarkProductCodes.Count
				   && Order.Client.IsNewEdoProcessing)
				{
					yield return new ValidationResult(
						"Для перепродажи должны быть отсканированы все коды.",
						new[] { nameof(item) });
				}

				var hasOtherSelfDeliveryDocumentsWithThisOrder = unitOfWork
					.GetAll<SelfDeliveryDocument>()
					.Any(x => x.Order.Id == Order.Id && x.Id != Id);

				if(hasOtherSelfDeliveryDocumentsWithThisOrder)
				{
					yield return new ValidationResult(
						$"Уже есть документ с заказом {Order.Id}",
						new[] { nameof(item) });
				}

				var hasOrderEdoRequest = unitOfWork
					.GetAll<OrderEdoRequest>()
					.Any(x => x.Order.Id == Order.Id && x.Id != Id);

				if(hasOrderEdoRequest)
				{
					yield return new ValidationResult(
						"Нельзя изменять документ самовывоза, по которому уже есть заявка на отправку документов заказа по ЭДО.",
						new[] { nameof(item) });
				}
			}
		}

		#region Функции

		/// <summary>
		/// Заполнение строк самовывоза по заказу
		/// </summary>
		public virtual void FillByOrder()
		{
			ObservableItems.Clear();
			if(Order == null)
			{
				return;
			}

			foreach(var orderItem in Order.OrderItems)
			{
				if(!Nomenclature
					.GetCategoriesForShipment()
					.Contains(orderItem.Nomenclature.Category))
				{
					continue;
				}

				if(!ObservableItems.Any(i => i.Nomenclature == orderItem.Nomenclature))
				{
					ObservableItems.Add(
						new SelfDeliveryDocumentItem
						{
							Document = this,
							Nomenclature = orderItem.Nomenclature,
							OrderItem = orderItem,
							OrderEquipment = null,
							Amount = GetNomenclaturesCountInOrder(orderItem.Nomenclature)
						});
				}

			}

			foreach(var orderEquipment in Order.OrderEquipments
				.Where(x => x.Direction == Direction.Deliver))
			{
				if(!ObservableItems.Any(i => i.Nomenclature == orderEquipment.Nomenclature))
				{
					ObservableItems.Add(
						new SelfDeliveryDocumentItem
						{
							Document = this,
							Nomenclature = orderEquipment.Nomenclature,
							OrderItem = null,
							OrderEquipment = orderEquipment,
							Amount = GetNomenclaturesCountInOrder(orderEquipment.Nomenclature)
						});
				}
			}

			if(!ReturnedItems.Any(x => x.Id != 0))
			{
				ReturnedItems = Order.OrderEquipments
					.Where(x => x.Direction == Direction.PickUp)
					.GroupBy(x => (x.Nomenclature, x.DirectionReason, x.OwnType))
					.ToDictionary(x => x.Key, x => x.ToList())
					.Select(x => new SelfDeliveryDocumentReturned
					{
						Document = this,
						Nomenclature = x.Key.Nomenclature,
						ActualCount = 0,
						Amount = x.Value.Sum(e => e.Count),
						Direction = Direction.PickUp,
						DirectionReason = x.Key.DirectionReason,
						OwnType = x.Key.OwnType
					})
					.ToList();
			}
		}

		/// <summary>
		/// Получение количества номенклатуры в заказе
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual decimal GetNomenclaturesCountInOrder(Nomenclature item)
		{
			decimal count = Order.OrderItems
				.Where(i => i.Nomenclature == item)
				.Sum(i => i.Count);

			count += Order.OrderEquipments
				.Where(e => e.Nomenclature == item
					&& e.Direction == Direction.Deliver)
				.Sum(e => e.Count);

			return count;
		}

		/// <summary>
		/// Получение количества возвратов оборудования в заказе
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual decimal GetEquipmentReturnsCountInOrder(Nomenclature item)
		{
			decimal count = Order.OrderEquipments
				.Where(e => e.Nomenclature == item
					&& e.Direction == Direction.PickUp)
				.Sum(e => e.Count);

			return count;
		}

		/// <summary>
		/// Обновление количества номенклатуры на складе
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="stockRepository"></param>
		public virtual void UpdateStockAmount(
			IUnitOfWork uow,
			IStockRepository stockRepository)
		{
			if(!Items.Any() || Warehouse == null)
			{
				return;
			}

			var nomenclatureIds = Items.Select(x => x.Nomenclature.Id).ToArray();
			var inStock =
				stockRepository.NomenclatureInStock(uow, nomenclatureIds, new[] { Warehouse.Id }, TimeStamp);

			foreach(var item in Items)
			{
				inStock.TryGetValue(item.Nomenclature.Id, out var stockValue);
				item.AmountInStock = stockValue;
			}
		}

		/// <summary>
		/// Инициализация значений по умолчанию
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="nomenclatureRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void InitializeDefaultValues(
			IUnitOfWork uow,
			INomenclatureRepository nomenclatureRepository)
		{
			if(nomenclatureRepository == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureRepository));
			}

			_defBottleId = nomenclatureRepository.GetDefaultBottleNomenclature(uow).Id;
		}

		/// <summary>
		/// Обновление количества номенклатуры, которая уже была отгружена
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="nomenclatureRepository"></param>
		/// <param name="bottlesRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void UpdateAlreadyUnloaded(
			IUnitOfWork uow,
			INomenclatureRepository nomenclatureRepository,
			IBottlesRepository bottlesRepository)
		{
			if(nomenclatureRepository == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureRepository));
			}

			if(bottlesRepository == null)
			{
				throw new ArgumentNullException(nameof(bottlesRepository));
			}

			if(Order != null)
			{
				ReturnedTareBefore = bottlesRepository
					.GetEmptyBottlesFromClientByOrder(uow, nomenclatureRepository, Order, Id);
			}

			TareToReturn = (int)ReturnedItems
				.Where(r => r.Nomenclature.Id == _defBottleId)
				.Sum(x => x.Amount);

			if(!Items.Any() || Order == null)
			{
				return;
			}

			var inUnloaded = ScopeProvider.Scope
				.Resolve<ISelfDeliveryRepository>()
				.NomenclatureUnloaded(uow, Order, this);

			foreach(var item in Items)
			{
				if(inUnloaded.ContainsKey(item.Nomenclature.Id))
				{
					item.AmountUnloaded = inUnloaded[item.Nomenclature.Id];
				}
			}
		}

		/// <summary>
		/// Обновление операций по самовывозу
		/// </summary>
		/// <param name="uow"></param>
		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			foreach(var item in Items)
			{
				if(item.Amount == 0 && item.GoodsAccountingOperation != null)
				{
					uow.Delete(item.GoodsAccountingOperation);
					item.GoodsAccountingOperation = null;
				}

				if(item.Amount != 0)
				{
					if(item.GoodsAccountingOperation != null)
					{
						item.UpdateOperation(Warehouse);
					}
					else
					{
						item.CreateOperation(Warehouse, TimeStamp);
					}
				}
			}
		}

		/// <summary>
		/// Обновление операций по возврату
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="goodsReceptions"></param>
		/// <param name="nomenclatureRepository"></param>
		/// <param name="bottlesRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void UpdateReceptions(
			IUnitOfWork uow,
			IList<GoodsReceptionVMNode> goodsReceptions,
			INomenclatureRepository nomenclatureRepository,
			IBottlesRepository bottlesRepository)
		{
			if(nomenclatureRepository == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureRepository));
			}

			if(bottlesRepository == null)
			{
				throw new ArgumentNullException(nameof(bottlesRepository));
			}

			if(Warehouse != null && Warehouse.CanReceiveBottles)
			{
				UpdateReturnedOperation(uow, _defBottleId, TareToReturn);
				var emptyBottlesAlreadyReturned = bottlesRepository.GetEmptyBottlesFromClientByOrder(uow, nomenclatureRepository, Order, Id);
				Order.ReturnedTare = emptyBottlesAlreadyReturned + TareToReturn;
			}

			if(Warehouse != null && Warehouse.CanReceiveEquipment)
			{
				foreach(GoodsReceptionVMNode item in goodsReceptions)
				{
					UpdateReturnedOperation(uow, item.NomenclatureId, item.Amount, item.OwnType, item.DirectionReason);
				}
			}
		}

		/// <summary>
		/// Обновление операций по возврату
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="returnedNomenclatures"></param>
		public virtual void UpdateReturnedOperations(
			IUnitOfWork uow,
			Dictionary<int, decimal> returnedNomenclatures)
		{
			foreach(var returned in returnedNomenclatures)
			{
				UpdateReturnedOperation(uow, returned.Key, returned.Value);
			}
		}

		/// <summary>
		/// Обновление операций по возврату
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="returnedNomenclaureId"></param>
		/// <param name="returnedNomenclaureQuantity"></param>
		/// <param name="ownType"></param>
		/// <param name="directionReason"></param>
		private void UpdateReturnedOperation(
			IUnitOfWork uow,
			int returnedNomenclaureId,
			decimal returnedNomenclaureQuantity,
			OwnTypes? ownType = null,
			DirectionReason? directionReason = null)
		{
			var items = ReturnedItems.Where(x => x.Nomenclature.Id == returnedNomenclaureId);

			if(ownType.HasValue)
			{
				items = items.Where(x => x.OwnType == ownType.Value);
			}

			if(directionReason.HasValue)
			{
				items = items.Where(x => x.DirectionReason == directionReason.Value);
			}

			var itemsToRemove = new List<SelfDeliveryDocumentReturned>();

			if(!items.Any())
			{
				if(returnedNomenclaureQuantity != 0)
				{
					var item = new SelfDeliveryDocumentReturned
					{
						Amount = returnedNomenclaureQuantity,
						Document = this,
						Nomenclature = uow.GetById<Nomenclature>(returnedNomenclaureId)
					};
					item.CreateOperation(Warehouse, Order.Client, TimeStamp);
					ReturnedItems.Add(item);
				}

				return;
			}

			foreach(var item in items)
			{
				if(returnedNomenclaureQuantity == 0)
				{
					itemsToRemove.Add(item);
				}
				else if(returnedNomenclaureQuantity != 0)
				{
					item.Amount = returnedNomenclaureQuantity;

					if(item.Id == 0)
					{
						item.CreateOperation(Warehouse, Order.Client, TimeStamp);
					}
					else
					{
						item.UpdateOperation(Warehouse, Order.Client);
					}
				}
			}

			foreach(var item in itemsToRemove)
			{
				ReturnedItems.Remove(item);
			}
		}

		/// <summary>
		/// Проверка самовывоза на возможность закрытия заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="nomenclatureSettings"></param>
		/// <param name="routeListItemRepository"></param>
		/// <param name="selfDeliveryRepository"></param>
		/// <param name="cashRepository"></param>
		/// <param name="callTaskWorker"></param>
		/// <returns></returns>
		public virtual bool FullyShiped(
			IUnitOfWork uow,
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			ICallTaskWorker callTaskWorker)
		{
			//Проверка текущего документа
			return Order.TryCloseSelfDeliveryOrderWithCallTask(
				uow,
				nomenclatureSettings,
				routeListItemRepository,
				selfDeliveryRepository,
				cashRepository,
				callTaskWorker,
				this);
		}

		#endregion
	}
}
