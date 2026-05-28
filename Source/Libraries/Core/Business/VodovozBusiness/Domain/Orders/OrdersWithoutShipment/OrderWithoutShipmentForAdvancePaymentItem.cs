using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки счета без отгрузки на предоплату",
		Nominative = "строка счета без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePaymentItem : PropertyChangedBase, IDomainObject, IDiscount
	{
		private IObservableList<DiscountReason> _discountReasons = new ObservableList<DiscountReason>();
		private bool _isAlternativePrice;

		public virtual int Id { get; set; }

		OrderWithoutShipmentForAdvancePayment orderWithoutDeliveryForAdvancePayment;
		[Display(Name = "Счет без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutDeliveryForAdvancePayment {
			get => orderWithoutDeliveryForAdvancePayment;
			set => SetField(ref orderWithoutDeliveryForAdvancePayment, value);
		}

		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set {
				if(SetField(ref nomenclature, value)) {
					CalculateVATType();
				}
			}
		}

		bool isUserPrice;
		[Display(Name = "Цена установлена пользователем")]
		public virtual bool IsUserPrice {
			get => isUserPrice;
			set => SetField(ref isUserPrice, value);
		}
		
		decimal price;
		[Display(Name = "Цена")]
		public virtual decimal Price {
			get => price;
			set {
				//Если цена не отличается от той которая должна быть по прайсам в 
				//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
				IsUserPrice = value != GetPriceByTotalCount() && value != 0;
				if(IsUserPrice)
					IsUserPrice = value != GetPriceByTotalCount() && value != 0;

				if(SetField(ref price, value)) {
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		decimal count = -1;
		[Display(Name = "Количество")]
		public virtual decimal Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					OrderWithoutDeliveryForAdvancePayment?.RecalculateItemsPrice();
					RecalculateDiscount();
					RecalculateVAT();
				}
			}
		}

		decimal? includeNDS;
		[Display(Name = "Включая НДС")]
		public virtual decimal? IncludeNDS {
			get => includeNDS;
			set => SetField(ref includeNDS, value);
		}

		private bool isDiscountInMoney;
		[Display(Name = "Скидка деньгами?")]
		public virtual bool IsDiscountInMoney {
			get => isDiscountInMoney;
			set {
				if(SetField(ref isDiscountInMoney, value))
					RecalculateVAT();
			}
		}

		private decimal discount;
		[Display(Name = "Процент скидки на товар")]
		public virtual decimal Discount {
			get => discount;
			set {
				if(value != discount && value == 0) {
					DiscountReasons.Clear();
				}
				if(SetField(ref discount, value)) {
					RecalculateVAT();
				}
			}
		}

		decimal? valueAddedTax;
		[Display(Name = "НДС на момент создания заказа")]
		public virtual decimal? ValueAddedTax {
			get => valueAddedTax;
			set => SetField(ref valueAddedTax, value);
		}

		private decimal discountMoney;
		[Display(Name = "Скидка на товар в деньгах")]
		public virtual decimal DiscountMoney {
			get => discountMoney;
			set {
				if(value != discountMoney && value == 0) {
					DiscountReasons.Clear();
				}
				if(SetField(ref discountMoney, value))
					RecalculateVAT();
			}
		}

		private decimal discountByStock;
		[Display(Name = "Скидка по акции")]
		public virtual decimal DiscountByStock {
			get => discountByStock;
			set => SetField(ref discountByStock, value);
		}

		[Display(Name = "Основания скидок на товар")]
		public virtual IObservableList<DiscountReason> DiscountReasons {
			get => _discountReasons;
			set => SetField(ref _discountReasons, value);
		}

		[Display(Name = "Альтернативная цена?")]
		public virtual bool IsAlternativePrice
		{
			get => _isAlternativePrice;
			set => SetField(ref _isAlternativePrice, value);
		}

		int RentEquipmentCount {
			get {
				return 0;
			}
		}

		int RentTime {
			get {
				return 0;
			}
		}

		public virtual decimal Sum => Price * Count - DiscountMoney;

		public virtual decimal ManualChangingDiscount {
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set {
				CalculateAndSetDiscount(value);
				if(DiscountByStock != 0) {
					DiscountByStock = 0;
					DiscountReasons.Clear();
				}
			}
		}

		public virtual bool IsRentCategory => RentEquipmentCount > 0;

		public virtual string RentString {
			get {
				int rentCount = RentTime;
				int count = RentEquipmentCount;

				if(rentCount != 0)
					return string.Format($"{count}*{rentCount}");
				return string.Empty;
			}
		}

		public virtual bool CanEditPrice {
			get {
				return Nomenclature.GetCategoriesWithEditablePrice().Contains(Nomenclature.Category);
			}
		}

		public virtual string NomenclatureString => Nomenclature != null ? Nomenclature.Name : string.Empty;

		public virtual decimal DiscountSetter {
			get => IsDiscountInMoney ? DiscountMoney : Discount;
			set => CalculateAndSetDiscount(value);
		}

		/// <summary>
		/// Текущее количество товара
		/// </summary>
		public decimal CurrentCount => Count;

		private void RecalculateDiscount()
		{
			if(!NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(DiscountMoney))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Discount))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Price))
			   || !NHibernate.NHibernateUtil.IsPropertyInitialized(this, nameof(Count))
			   || (OrderWithoutDeliveryForAdvancePayment == null || !NHibernate.NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment.OrderWithoutDeliveryForAdvancePaymentItems))) {
				return;
			}

			if(Count == 0)
			{
				ClearDiscounts();
			}
			else if(DiscountReasons.Any())
			{
				RecalculateTotalDiscountFromReasons();
			}
			else
			{
				var discount = IsDiscountInMoney
					? DiscountMoney
					: Discount;

				CalculateAndSetDiscount(discount);
			}
		}

		public virtual void AddDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason)
		{
			if(discountReason != null && !IsDiscountReasonAdded(discountReason))
			{
				DiscountReasons.Add(discountReason);
			}

			RecalculateTotalDiscountFromReasons();
		}

		public virtual bool IsDiscountValueCanBeAdded(bool isDiscountInMoney, decimal discount)
		{
			var isCalculateInPercent =
				DiscountReasons.All(x => x.ValueType == DiscountUnits.percent) && !isDiscountInMoney;

			if(isCalculateInPercent)
			{
				var totalPercentDiscount = DiscountReasons.Sum(x => x.Value) + discount;
				if(totalPercentDiscount > 100)
				{
					return false;
				}
			}

			var alreadyAddedDiscount = CalculateTotalDiscountInMoneyFromAddedReasons();
			var discountMoneyToAdd = isDiscountInMoney ? discount : CurrentRawPrice * discount / 100;

			return discountMoneyToAdd + alreadyAddedDiscount <= CurrentRawPrice;
		}

		public virtual bool IsDiscountReasonAdded(DiscountReason discountReason)
		{
			if(discountReason is null)
			{
				throw new ArgumentNullException(nameof(discountReason));
			}

			return DiscountReasons.Any(x => x.Id == discountReason.Id);
		}

		private void RecalculateTotalDiscountFromReasons()
		{
			var currentPrice = CurrentRawPrice;
			var totalDiscountMoney = CalculateTotalDiscountInMoneyFromAddedReasons();

			if(totalDiscountMoney > currentPrice)
			{
				totalDiscountMoney = currentPrice;
			}

			DiscountMoney = totalDiscountMoney;
			Discount = currentPrice > 0 ? (100 * DiscountMoney) / currentPrice : 0;

			RecalculateVAT();
		}

		private decimal CurrentRawPrice => Price * CurrentCount;

		private decimal CalculateTotalDiscountInMoneyFromAddedReasons()
		{
			decimal currentPrice = CurrentRawPrice;

			decimal totalPercentDiscount = 0;
			decimal totalMoneyDiscount = 0;

			foreach(var reason in DiscountReasons)
			{
				if(reason.ValueType is DiscountUnits.money)
				{
					totalMoneyDiscount += reason.Value;
				}
				else
				{
					totalPercentDiscount += reason.Value;
				}
			}

			if(totalPercentDiscount > 100)
			{
				totalPercentDiscount = 100;
			}

			decimal discountFromPercent = currentPrice * (totalPercentDiscount / 100);
			decimal totalDiscountMoney = discountFromPercent + totalMoneyDiscount;

			return totalDiscountMoney;
		}

		public virtual void RemoveDiscount(int discountReasonId)
		{
			if(!DiscountReasons.Any())
			{
				return;
			}

			var reasonsToRemove = DiscountReasons.Where(r => r.Id == discountReasonId).ToList();

			foreach(var reason in reasonsToRemove)
			{
				DiscountReasons.Remove(reason);
			}

			RecalculateTotalDiscountFromReasons();
		}

		public virtual void ClearDiscounts()
		{
			DiscountReasons.Clear();
			DiscountMoney = 0;
			Discount = 0;
		}

		public virtual void RecalculatePrice()
		{
			if(IsUserPrice)
				return;

			Price = GetPriceByTotalCount();
		}

		private void CalculateAndSetDiscount(decimal value)
		{
			if((Price * Count) == 0) {
				DiscountMoney = 0;
				Discount = 0;
				return;
			}
			if(IsDiscountInMoney) {
				DiscountMoney = value > Price * Count ? Price * Count : (value < 0 ? 0 : value);
				Discount = (100 * DiscountMoney) / (Price * Count);
			} else {
				Discount = value > 100 ? 100 : (value < 0 ? 0 : value);
				DiscountMoney = Price * Count * Discount / 100;
			}
		}

		public virtual decimal GetPriceByTotalCount()
		{
			if(Nomenclature != null)
			{
				var curCount = Nomenclature.IsWater19L ? OrderWithoutDeliveryForAdvancePayment.GetTotalWater19LCount() : Count;
				var canApplyAlternativePrice = OrderWithoutDeliveryForAdvancePayment.HasPermissionsForAlternativePrice
				                               && Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount);

				if(Nomenclature.DependsOnNomenclature == null)
					return Nomenclature.GetPrice(curCount, canApplyAlternativePrice);
				if(Nomenclature.IsWater19L)
					return Nomenclature.DependsOnNomenclature.GetPrice(curCount, canApplyAlternativePrice);
			}
			return 0m;
		}

		public virtual void CalculateVATType()
		{
			if(!NHibernateUtil.IsInitialized(Nomenclature)) {
				NHibernateUtil.Initialize(Nomenclature);
			}
			if(OrderWithoutDeliveryForAdvancePayment == null) {
				return;
			}
			
			if(!NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment)) {
				NHibernateUtil.Initialize(OrderWithoutDeliveryForAdvancePayment);
			}

			var organization = OrderWithoutDeliveryForAdvancePayment.Organization;

			var vatRateVersion = Nomenclature.GetEffectiveVatRateVersion(organization, OrderWithoutDeliveryForAdvancePayment.DocumentDate);

			if (vatRateVersion == null)
			{
				throw new InvalidOperationException(
					$"У товара #{Nomenclature.Id} отсутствует версия НДС на дату доставки счета #{OrderWithoutDeliveryForAdvancePayment.DocumentDate}");
			}

			ValueAddedTax = CanUseVAT() ? vatRateVersion.VatRate.VatNumericValue : 0;
		}
		
		private void RecalculateVAT()
		{
			if(OrderWithoutDeliveryForAdvancePayment == null) {
				return;
			}

			if(!NHibernateUtil.IsInitialized(OrderWithoutDeliveryForAdvancePayment)) {
				NHibernateUtil.Initialize(OrderWithoutDeliveryForAdvancePayment);
			}
			
			if(!CanUseVAT()) {
				IncludeNDS = null;
				return;
			}

			if(CanUseVAT() && ValueAddedTax.HasValue) {
				IncludeNDS = Math.Round(Sum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
			}
		}

		private bool CanUseVAT()
		{
			var canUseVat = true;
			var organization = OrderWithoutDeliveryForAdvancePayment.Organization;
			
			if(organization != null) {
				canUseVat = organization.GetActualVatRateVersion(OrderWithoutDeliveryForAdvancePayment.DocumentDate)?.VatRate.VatNumericValue != 0;
			}

			return canUseVat;
		}

		/// <summary>
		/// Наименования оснований скидки через запятую
		/// </summary>
		public virtual string DiscountReasonsNames =>
			string.Join(", ", DiscountReasons.Select(x => x.Name));

		public OrderWithoutShipmentForAdvancePaymentItem() { }
	}
}
