using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "промонаборы",
		Nominative = "промонабор",
		Prepositional = "промонаборе",
		PrepositionalPlural = "промонаборах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class PromotionalSet : BusinessObjectBase<PromotionalSet>, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		private const int _onlineNameLimit = 45;
		private string _name;
		private DateTime _createDate;
		private bool _isArchive;
		private string _discountReasonInfo;
		private bool _canEditNomenclatureCount;
		private bool _promotionalSetForNewClients;
		private string _onlineName;
		private int? _bottlesCountForCalculatingDeliveryPrice;
		
		private IList<PromotionalSetItem> _promotionalSetItems = new List<PromotionalSetItem>();
		private GenericObservableList<PromotionalSetItem> _observablePromotionalSetItems;
		private IList<PromotionalSetActionBase> _promotionalSetActions = new List<PromotionalSetActionBase>();
		private GenericObservableList<PromotionalSetActionBase> _observablePromotionalSetActions;
		private IList<Order> _orders = new List<Order>();
		private GenericObservableList<Order> _observableOrders;
		private IList<PromotionalSetOnlineParameters> _promotionalSetOnlineParameters = new List<PromotionalSetOnlineParameters>();

		#region Cвойства

		public virtual int Id { get; set; }

		[Display(Name = "Название набора")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}
		
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		[Display(Name = "Согласованная акция")]
		public virtual string DiscountReasonInfo
		{
			get => _discountReasonInfo;
			set => SetField(ref _discountReasonInfo, value);
		}
		
		[Display(Name = "Можно менять количество номенклатур")]
		public virtual bool CanEditNomenclatureCount
		{
			get => _canEditNomenclatureCount;
			set => SetField(ref _canEditNomenclatureCount, value);
		}

		[Display(Name = "Набор для новых клиентов")]
		public virtual bool PromotionalSetForNewClients
		{
			get => _promotionalSetForNewClients;
			set => SetField(ref _promotionalSetForNewClients, value);
		}
		
		[Display(Name = "Название для ИПЗ")]
		public virtual string OnlineName
		{
			get => _onlineName;
			set => SetField(ref _onlineName, value);
		}
		
		[Display(Name = "Количество бутылей для расчета платной доставки")]
		public virtual int? BottlesCountForCalculatingDeliveryPrice
		{
			get => _bottlesCountForCalculatingDeliveryPrice;
			set => SetField(ref _bottlesCountForCalculatingDeliveryPrice, value);
		}
		
		[Display(Name = "Строки рекламного набора")]
		public virtual IList<PromotionalSetItem> PromotionalSetItems
		{
			get => _promotionalSetItems;
			set => SetField(ref _promotionalSetItems, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetItem> ObservablePromotionalSetItems =>
			_observablePromotionalSetItems ??
			(_observablePromotionalSetItems = new GenericObservableList<PromotionalSetItem>(_promotionalSetItems));

		[Display(Name = "Использован для заказов")]
		public virtual IList<Order> Orders
		{
			get => _orders;
			set => SetField(ref _orders, value, () => Orders);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Order> ObservableOrders => 
			_observableOrders ?? (_observableOrders = new GenericObservableList<Order>(Orders));
		
		[Display(Name = "Онлайн параметры промонабора")]
		public virtual IList<PromotionalSetOnlineParameters> PromotionalSetOnlineParameters
		{
			get => _promotionalSetOnlineParameters;
			set => SetField(ref _promotionalSetOnlineParameters, value);
		}

		#endregion

		public virtual string Title => $"Промонабор №{Id} \"{Name}\"";
		public virtual string ShortTitle => $"Промонабор \"{Name}\"";

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult(
					"Необходимо выбрать название набора",
					new[] { nameof(Name) }
				);
			}
			
			if(!string.IsNullOrEmpty(OnlineName) && OnlineName.Length > _onlineNameLimit)
			{
				yield return new ValidationResult($"Название для ИПЗ превышено на {OnlineName.Length - _onlineNameLimit}",
					new[] { nameof(Name) }
				);
			}

			if((!PromotionalSetItems.Any() || PromotionalSetItems.Any(i => i.Count <= 0)))
			{
				yield return new ValidationResult(
					"Необходимо выбрать номенклатуру",
					new[] { nameof(PromotionalSetItems) }
				);
			}

			if(PromotionalSetItems.Any(i => i.Count == 0))
			{
				yield return new ValidationResult(
					"Необходимо выбрать количество номенклатур, отличное от нуля",
					new[] { nameof(PromotionalSetItems) }
				);
			}

			if(PromotionalSetItems.Any(i => i.Discount < 0 || i.Discount > 100))
			{
				yield return new ValidationResult(
					"Скидка не может быть меньше 0 или больше 100%",
					new[] { nameof(PromotionalSetItems) }
				);
			}

			if(PromotionalSetItems.Any(i => i.Discount != 0 &&
											PromotionalSetActions.OfType<PromotionalSetActionFixPrice>()
												.Select(a => a.Nomenclature.Id).Contains(i.Nomenclature.Id)))
			{
				yield return new ValidationResult(
					"Нельзя выбрать скидку на номенклатуру, для которой уже была создана фиксированная цена",
					new[] { nameof(PromotionalSetItems) }
				);
			}
		}

		#endregion

		public virtual IList<PromotionalSetActionBase> PromotionalSetActions
		{
			get => _promotionalSetActions;
			set => SetField(ref _promotionalSetActions, value);
		}
		
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSetActionBase> ObservablePromotionalSetActions =>
			_observablePromotionalSetActions ?? (_observablePromotionalSetActions =
				new GenericObservableList<PromotionalSetActionBase>(_promotionalSetActions));

		public virtual bool IsValidForOrder(Order order, INomenclatureSettings nomenclatureSettings)
		{
			return !PromotionalSetActions.Any(a => !a.IsValidForOrder(order, nomenclatureSettings));
		}
	}
}
