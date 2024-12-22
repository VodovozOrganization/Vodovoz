using Autofac;
using Gamma.Utilities;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Goods
{
	public class Nomenclature : NomenclatureEntity, IArchivable, IValidatableObject
	{
		private bool _isNewBottle;
		private bool _isDefectiveBottle;
		private bool _isShabbyBottle;
		private decimal _length;
		private decimal _width;
		private decimal _height;

		private IList<NomenclaturePurchasePrice> _purchasePrices = new List<NomenclaturePurchasePrice>();
		private IList<NomenclatureCostPrice> _costPrices = new List<NomenclatureCostPrice>();
		private IList<NomenclatureInnerDeliveryPrice> _innerDeliveryPrices = new List<NomenclatureInnerDeliveryPrice>();
		private IList<AlternativeNomenclaturePrice> _alternativeNomenclaturePrices = new List<AlternativeNomenclaturePrice>();
		private GenericObservableList<NomenclaturePurchasePrice> _observablePurchasePrices;
		private GenericObservableList<NomenclatureCostPrice> _observableCostPrices;
		private GenericObservableList<NomenclatureInnerDeliveryPrice> _observableInnerDeliveryPrices;
		private GenericObservableList<NomenclaturePrice> _observableNomenclaturePrices;
		private GenericObservableList<AlternativeNomenclaturePrice> _observableAlternativeNomenclaturePrices;		
		private MobileAppNomenclatureOnlineCatalog _mobileAppNomenclatureOnlineCatalog;
		private VodovozWebSiteNomenclatureOnlineCatalog _vodovozWebSiteNomenclatureOnlineCatalog;
		private KulerSaleWebSiteNomenclatureOnlineCatalog _kulerSaleWebSiteNomenclatureOnlineCatalog;
		private NomenclatureOnlineGroup _nomenclatureOnlineGroup;
		private NomenclatureOnlineCategory _nomenclatureOnlineCategory;
		private IList<NomenclatureOnlineParameters> _nomenclatureOnlineParameters = new List<NomenclatureOnlineParameters>();
		private Folder1c _folder1;
		private User _createdBy;
		private EquipmentColors _equipmentColor;
		private EquipmentKind _kind;
		private Manufacturer _manufacturer;
		private RouteColumn _routeListColumn;
		private IList<NomenclaturePrice> _nomenclaturePrice = new List<NomenclaturePrice>();
		private FuelType _fuelType;
		private Nomenclature _dependsOnNomenclature;
		private OnlineStore _onlineStore;
		private ProductGroup _productGroup;
		private Counterparty _shipperCounterparty;
		private IObservableList<NomenclatureMinimumBalanceByWarehouse> _nomenclatureMinimumBalancesByWarehouse = new ObservableList<NomenclatureMinimumBalanceByWarehouse>();

		private NomenclatureCategory _category;

		#region Свойства

		/// <summary>
		/// Кем создана(пользователь)
		/// </summary>
		[Display(Name = "Кем создана")]
		public virtual User CreatedBy
		{
			get => _createdBy;
			set => SetField(ref _createdBy, value);
		}

		/// <summary>
		/// Папка в 1с
		/// </summary>
		[Display(Name = "Папка в 1с")]
		public virtual Folder1c Folder1C
		{
			get => _folder1;
			set => SetField(ref _folder1, value);
		}

		/// <summary>
		/// Объем номенклатуры, измеряемый в квадратных метрах
		/// </summary>
		[Display(Name = "Объём")]
		public virtual decimal Volume => Length * Width * Height / 1000000;    // 1 000 000


		/// <summary>
		/// Категория
		/// </summary>
		[Display(Name = "Категория")]
		public virtual new NomenclatureCategory Category
		{
			get => _category;
			set
			{
				if(SetField(ref _category, value))
				{
					if(!CategoriesWithSerial.Contains(Category))
					{
						IsSerial = false;
					}

					if(Category != NomenclatureCategory.water)
					{
						TareVolume = null;
					}

					if(!GetCategoriesWithSaleCategory().Contains(value))
					{
						SaleCategory = null;
					}
					if(value != NomenclatureCategory.master)
					{
						MasterServiceType = null;
					}
				}
			}
		}


		/// <summary>
		/// Тип выезда мастера
		/// </summary>
		[Display(Name = "Тип выезда мастера")]
		public virtual MasterServiceType? MasterServiceType
		{
			get => _masterServiceType;
			set => SetField(ref _masterServiceType, value);
		}

		/// <summary>
		/// Цвет оборудования
		/// </summary>
		[Display(Name = "Цвет оборудования")]
		public virtual EquipmentColors EquipmentColor
		{
			get => _equipmentColor;
			set => SetField(ref _equipmentColor, value);
		}

		/// <summary>
		/// Вид оборудования
		/// </summary>
		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind Kind
		{
			get => _kind;
			set => SetField(ref _kind, value);
		}

		/// <summary>
		/// Производитель
		/// </summary>
		[Display(Name = "Производитель")]
		public virtual Manufacturer Manufacturer
		{
			get => _manufacturer;
			set => SetField(ref _manufacturer, value);
		}

		/// <summary>
		/// Колонка МЛ
		/// </summary>
		[Display(Name = "Колонка МЛ")]
		public virtual RouteColumn RouteListColumn
		{
			get => _routeListColumn;
			set => SetField(ref _routeListColumn, value);
		}


		/// <summary>
		/// Цены
		/// </summary>
		[Display(Name = "Цены")]
		public virtual IList<NomenclaturePrice> NomenclaturePrice
		{
			get => _nomenclaturePrice;
			set => SetField(ref _nomenclaturePrice, value);
		}

		/// <summary>
		/// Альтернативные цены
		/// </summary>
		[Display(Name = "Альтернативные цены")]
		public virtual IList<AlternativeNomenclaturePrice> AlternativeNomenclaturePrices
		{
			get => _alternativeNomenclaturePrices;
			set => SetField(ref _alternativeNomenclaturePrices, value);
		}

		/// <summary>
		/// Это новая бутыль
		/// </summary>
		[Display(Name = "Это новая бутыль")]
		public virtual bool IsNewBottle
		{
			get => _isNewBottle;
			set
			{
				if(SetField(ref _isNewBottle, value) && _isNewBottle)
				{
					IsDefectiveBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		/// <summary>
		/// Это бракованая бутыль
		/// </summary>
		[Display(Name = "Это бракованая бутыль")]
		public virtual bool IsDefectiveBottle
		{
			get => _isDefectiveBottle;
			set
			{
				if(SetField(ref _isDefectiveBottle, value) && _isDefectiveBottle)
				{
					IsNewBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		/// <summary>
		/// Стройка
		/// </summary>
		[Display(Name = "Стройка")]
		public virtual bool IsShabbyBottle
		{
			get => _isShabbyBottle;
			set
			{
				if(SetField(ref _isShabbyBottle, value) && _isShabbyBottle)
				{
					IsNewBottle = false;
					IsDefectiveBottle = false;
				}
			}
		}


		/// <summary>
		/// Тип топлива
		/// </summary>
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType
		{
			get => _fuelType;
			set => SetField(ref _fuelType, value);
		}

		/// <summary>
		/// Влияющая номенклатура
		/// </summary>
		[Display(Name = "Влияющая номенклатура")]
		public virtual Nomenclature DependsOnNomenclature
		{
			get => _dependsOnNomenclature;
			set => SetField(ref _dependsOnNomenclature, value);
		}


		/// <summary>
		/// Группа товаров
		/// </summary>
		[Display(Name = "Группа товаров")]
		public virtual ProductGroup ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}


		/// <summary>
		/// Интернет-магазин
		/// </summary>
		[Display(Name = "Интернет-магазин")]
		public virtual OnlineStore OnlineStore
		{
			get => _onlineStore;
			set => SetField(ref _onlineStore, value);
		}


		/// <summary>
		/// Параметры номенклатуры для ИПЗ
		/// </summary>
		public virtual IList<NomenclatureOnlineParameters> NomenclatureOnlineParameters
		{
			get => _nomenclatureOnlineParameters;
			set => SetField(ref _nomenclatureOnlineParameters, value);
		}

		/// <summary>
		/// Цены закупки ТМЦ
		/// </summary>
		[Display(Name = "Цены закупки ТМЦ")]
		public virtual IList<NomenclaturePurchasePrice> PurchasePrices
		{
			get => _purchasePrices;
			set => SetField(ref _purchasePrices, value);
		}

		public virtual GenericObservableList<NomenclaturePurchasePrice> ObservablePurchasePrices =>
			_observablePurchasePrices ?? (_observablePurchasePrices = new GenericObservableList<NomenclaturePurchasePrice>(PurchasePrices));

		/// <summary>
		/// Себестоимость ТМЦ
		/// </summary>
		[Display(Name = "Себестоимость ТМЦ")]
		public virtual IList<NomenclatureCostPrice> CostPrices
		{
			get => _costPrices;
			set => SetField(ref _costPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureCostPrice> ObservableCostPrices =>
			_observableCostPrices ?? (_observableCostPrices = new GenericObservableList<NomenclatureCostPrice>(CostPrices));

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclaturePrice> ObservableNomenclaturePrices
		{
			get => _observableNomenclaturePrices ?? (_observableNomenclaturePrices = new GenericObservableList<NomenclaturePrice>(NomenclaturePrice));
			set => _observableNomenclaturePrices = value;
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AlternativeNomenclaturePrice> ObservableAlternativeNomenclaturePrices
		{
			get => _observableAlternativeNomenclaturePrices ?? (_observableAlternativeNomenclaturePrices = new GenericObservableList<AlternativeNomenclaturePrice>(AlternativeNomenclaturePrices));
			set => _observableAlternativeNomenclaturePrices = value;
		}

		/// <summary>
		/// Стоимости доставки ТМЦ на склад
		/// </summary>
		[Display(Name = "Стоимости доставки ТМЦ на склад")]
		public virtual IList<NomenclatureInnerDeliveryPrice> InnerDeliveryPrices
		{
			get => _innerDeliveryPrices;
			set => SetField(ref _innerDeliveryPrices, value);
		}

		/// <summary>
		/// Минимальный остаток на складе
		/// </summary>
		[Display(Name = "Минимальный остаток на складе")]
		public virtual IObservableList<NomenclatureMinimumBalanceByWarehouse> NomenclatureMinimumBalancesByWarehouse
		{
			get => _nomenclatureMinimumBalancesByWarehouse;
			set => SetField(ref _nomenclatureMinimumBalancesByWarehouse, value);
		}

		public virtual GenericObservableList<NomenclatureInnerDeliveryPrice> ObservableInnerDeliveryPrices =>
			_observableInnerDeliveryPrices ?? (_observableInnerDeliveryPrices = new GenericObservableList<NomenclatureInnerDeliveryPrice>(InnerDeliveryPrices));

		/// <summary>
		/// Длина номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Длина")]
		public virtual decimal Length
		{
			get => _length;
			set
			{
				if(SetField(ref _length, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		/// <summary>
		/// Ширина номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Ширина")]
		public virtual decimal Width
		{
			get => _width;
			set
			{
				if(SetField(ref _width, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		/// <summary>
		/// Высота номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Высота")]
		public virtual decimal Height
		{
			get => _height;
			set
			{
				if(SetField(ref _height, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		#endregion Свойства

		#region Свойства товаров для магазина

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		public virtual Counterparty ShipperCounterparty
		{
			get => _shipperCounterparty;
			set => SetField(ref _shipperCounterparty, value);
		}

		#endregion Свойства товаров для магазина

		#region Онлайн характеристики для ИПЗ

		/// <summary>
		/// Онлайн каталог в мобильном приложении
		/// </summary>
		[Display(Name = "Онлайн каталог в мобильном приложении")]
		public virtual MobileAppNomenclatureOnlineCatalog MobileAppNomenclatureOnlineCatalog
		{
			get => _mobileAppNomenclatureOnlineCatalog;
			set => SetField(ref _mobileAppNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн каталог на сайте ВВ
		/// </summary>
		[Display(Name = "Онлайн каталог на сайте ВВ")]
		public virtual VodovozWebSiteNomenclatureOnlineCatalog VodovozWebSiteNomenclatureOnlineCatalog
		{
			get => _vodovozWebSiteNomenclatureOnlineCatalog;
			set => SetField(ref _vodovozWebSiteNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн каталог на сайте Кулер Сэйл
		/// </summary>
		[Display(Name = "Онлайн каталог на сайте Кулер Сэйл")]
		public virtual KulerSaleWebSiteNomenclatureOnlineCatalog KulerSaleWebSiteNomenclatureOnlineCatalog
		{
			get => _kulerSaleWebSiteNomenclatureOnlineCatalog;
			set => SetField(ref _kulerSaleWebSiteNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн вид товара
		/// </summary>
		[Display(Name = "Онлайн вид товара")]
		public virtual NomenclatureOnlineGroup NomenclatureOnlineGroup
		{
			get => _nomenclatureOnlineGroup;
			set => SetField(ref _nomenclatureOnlineGroup, value);
		}

		/// <summary>
		/// Онлайн тип товара
		/// </summary>
		[Display(Name = "Онлайн тип товара")]
		public virtual NomenclatureOnlineCategory NomenclatureOnlineCategory
		{
			get => _nomenclatureOnlineCategory;
			set => SetField(ref _nomenclatureOnlineCategory, value);
		}

		#endregion Онлайн характеристики для ИПЗ

		#region Рассчетные

		public virtual string CategoryString => Category.GetEnumTitle();

		public virtual string ShortOrFullName => string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;

		public virtual bool IsWater19L =>
			Category == NomenclatureCategory.water
			&& TareVolume.HasValue
			&& TareVolume.Value == Core.Domain.Goods.TareVolume.Vol19L;

		public override string ToString() => $"id = {Id} Name = {Name}";

		#endregion Рассчетные

		#region Методы

		public virtual void SetNomenclatureCreationInfo(IUserRepository userRepository)
		{
			if(Id == 0 && !CreateDate.HasValue)
			{
				CreateDate = DateTime.Now;
				CreatedBy = userRepository.GetCurrentUser(UoW);
			}
		}

		public virtual decimal GetPrice(decimal? itemsCount, bool useAlternativePrice = false)
		{
			if(itemsCount < 1)
			{
				itemsCount = 1;
			}

			decimal price = 0m;
			if(DependsOnNomenclature != null)
			{
				price = DependsOnNomenclature.GetPrice(itemsCount, useAlternativePrice);
			}
			else
			{
				var nomPrice = (useAlternativePrice
						? AlternativeNomenclaturePrices.Cast<NomenclaturePriceBase>()
						: NomenclaturePrice.Cast<NomenclaturePriceBase>())
					.OrderByDescending(p => p.MinCount)
					.FirstOrDefault(p => p.MinCount <= itemsCount);
				price = nomPrice?.Price ?? 0;
			}
			return price;
		}

		/// <summary>
		/// Cоздает новый Guid. Uow необходим для сохранения созданного Guid в базу.
		/// </summary>
		public virtual void CreateGuidIfNotExist(IUnitOfWork uow)
		{
			if(OnlineStoreGuid == null)
			{
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
		}

		public virtual bool IsFromOnlineShopGroup(int idOfOnlineShopGroup)
		{
			ProductGroup parent = ProductGroup;
			while(parent != null)
			{
				if(parent.Id == idOfOnlineShopGroup)
				{
					return true;
				}

				parent = parent.Parent;
			}
			return false;
		}

		public virtual decimal GetPurchasePriceOnDate(DateTime date)
		{
			var purchasePrice =
				PurchasePrices
				.Where(p => p.StartDate <= date && (p.EndDate == null || p.EndDate >= date))
				.Select(p => p.PurchasePrice)
				.FirstOrDefault();

			return purchasePrice;
		}

		#endregion Методы

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(
				typeof(INomenclatureRepository)) is INomenclatureRepository nomenclatureRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(nomenclatureRepository)}");
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Название номенклатуры должно быть заполнено.", new[] { nameof(Name) });
			}
			else if(Name.Length > 220)
			{
				yield return new ValidationResult(
					"Превышено максимальное количество символов в названии (220).", new[] { nameof(Name) });
			}

			if(string.IsNullOrWhiteSpace(OfficialName))
			{
				yield return new ValidationResult(
					"Официальное название номенклатуры должно быть заполнено.", new[] { nameof(OfficialName) });
			}
			else if(Name.Length > 220)
			{
				yield return new ValidationResult(
					"Превышено максимальное количество символов в официальном названии (220).", new[] { nameof(OfficialName) });
			}

			if(CategoriesWithWeightAndVolume.Contains(Category) && (Length == 0 || Width == 0 || Height == 0 || Weight == 0))
			{
				yield return new ValidationResult("Длина, ширина, высота и вес номенклатуры обязательны для заполнения",
					new[] { nameof(Length), nameof(Width), nameof(Height), nameof(Weight) });
			}

			if(Length < 0 || Width < 0 || Height < 0 || Weight < 0)
			{
				yield return new ValidationResult("Длина, ширина, высота и вес номенклатуры должны быть положительными",
					new[] { nameof(Length), nameof(Width), nameof(Height), nameof(Weight) });
			}

			if(Folder1C == null)
			{
				yield return new ValidationResult(
					"Папка 1С обязательна для заполнения", new[] { nameof(Folder1C) });
			}

			if(string.IsNullOrWhiteSpace(Code1c))
			{
				yield return new ValidationResult(
					"Код 1С обязателен для заполнения", new[] { nameof(Code1c) });
			}

			if(Category == NomenclatureCategory.equipment && Kind == null)
			{
				yield return new ValidationResult(
					"Не указан вид оборудования.",
					new[] { nameof(Kind) });
			}

			if(GetCategoriesWithSaleCategory().Contains(_category) && SaleCategory == null)
			{
				yield return new ValidationResult(
					"Не указана \"Доступность для продажи\"",
					new[] { nameof(SaleCategory) }
				);
			}

			if(Category == NomenclatureCategory.deposit && TypeOfDepositCategory == null)
			{
				yield return new ValidationResult(
					"Не указан тип залога.",
					new[] { nameof(TypeOfDepositCategory) });
			}

			if(Category == NomenclatureCategory.water && !TareVolume.HasValue)
			{
				yield return new ValidationResult(
					"Не выбран объем тары",
					new[] { nameof(TareVolume) }
				);
			}

			if(Category == NomenclatureCategory.fuel && FuelType == null)
			{
				yield return new ValidationResult("Не выбран тип топлива");
			}

			if(Unit == null)
			{
				yield return new ValidationResult(
					"Не указаны единицы измерения",
					new[] { nameof(Unit) });
			}

			//Проверка зависимостей номенклатур #1: если есть зависимые
			if(DependsOnNomenclature != null)
			{
				IList<Nomenclature> dependedNomenclatures = nomenclatureRepository.GetDependedNomenclatures(UoW, this);

				if(dependedNomenclatures.Any())
				{
					string dependedNomenclaturesText = "Цена данной номенклатуры не может зависеть от другой номенклатуры, т.к. от данной номенклатуры зависят цены следующих номенклатур:\n";

					foreach(Nomenclature n in dependedNomenclatures)
					{
						dependedNomenclaturesText += $"{n.Id}: {n.OfficialName} ({n.CategoryString})\n";
					}

					yield return new ValidationResult(dependedNomenclaturesText, new[] { nameof(DependsOnNomenclature) });
				}

				if(DependsOnNomenclature.DependsOnNomenclature != null)
				{
					yield return new ValidationResult(
						$"Номенклатура '{DependsOnNomenclature.ShortOrFullName}' указанная в качеcтве основной для цен этой номеклатуры, сама зависит от '{DependsOnNomenclature.DependsOnNomenclature.ShortOrFullName}'",
						new[] { nameof(DependsOnNomenclature) });
				}
			}

			if(Code1c != null && Code1c.StartsWith(PrefixOfCode1c))
			{
				if(Code1c.Length != LengthOfCode1c)
				{
					yield return new ValidationResult(
						$"Код 1с с префиксом автоформирования '{PrefixOfCode1c}', должен содержать {LengthOfCode1c}-символов.",
						new[] { nameof(Code1c) });
				}

				var next = nomenclatureRepository.GetNextCode1c(UoW);
				if(string.Compare(Code1c, next) > 0)
				{
					yield return new ValidationResult(
						$"Код 1с использует префикс автоматического формирования кодов '{PrefixOfCode1c}'. При этом пропускает некоторое количество значений. Используйте в качестве следующего кода {next} или оставьте это поле пустым для автозаполенения.",
						new[] { nameof(Code1c) });
				}
			}

			if(DateTime.Now >= new DateTime(2019, 01, 01) && VAT == VAT.Vat18)
			{
				yield return new ValidationResult(
					"С 01.01.2019 ставка НДС 20%",
					new[] { nameof(VAT) }
				);
			}

			foreach(var purchasePrice in PurchasePrices)
			{
				foreach(var validationResult in purchasePrice.Validate(validationContext))
				{
					yield return validationResult;
				}
			}

			if(IsAccountableInTrueMark && string.IsNullOrWhiteSpace(Gtin))
			{
				yield return new ValidationResult("Должен быть заполнен GTIN для ТМЦ, подлежащих учёту в Честном знаке.",
					new[] { nameof(Gtin) });
			}

			if(Gtin?.Length < 8 || Gtin?.Length > 14)
			{
				yield return new ValidationResult("Длина GTIN должна быть от 8 до 14 символов",
					new[] { nameof(Gtin) });
			}

			if(ProductGroup == null)
			{
				yield return new ValidationResult("Должна быть выбрана принадлежность номенклатуры к группе товаров",
					new[] { nameof(ProductGroup) });
			}

			if((LengthOnline >= 0 && (!WidthOnline.HasValue || WidthOnline == 0 || !HeightOnline.HasValue || HeightOnline == 0))
				|| (WidthOnline >= 0 && (!LengthOnline.HasValue || LengthOnline == 0 || !HeightOnline.HasValue || HeightOnline == 0))
				|| (HeightOnline >= 0 && (!LengthOnline.HasValue || LengthOnline == 0 || !WidthOnline.HasValue || WidthOnline == 0)))
			{
				yield return new ValidationResult(
					"Габариты на вкладке Сайты и приложения должны быть либо пустыми, либо заполнены и больше 0",
					new[] { nameof(LengthOnline), nameof(WidthOnline), nameof(HeightOnline) });
			}
			
			if(WeightOnline == 0)
			{
				yield return new ValidationResult(
					"Вес на вкладке Сайты и приложения должен быть больше 0",
					new[] { nameof(WeightOnline) });
			}
			
			if(CoolingTemperatureFromOnline > CoolingTemperatureToOnline)
			{
				yield return new ValidationResult("Начальное значение температуры охлаждения не может быть больше конечного",
					new[] { nameof(CoolingTemperatureFromOnline), nameof(CoolingTemperatureToOnline) });
			}
			
			if(HeatingTemperatureFromOnline > HeatingTemperatureToOnline)
			{
				yield return new ValidationResult("Начальное значение температуры нагрева не может быть больше конечного",
					new[] { nameof(HeatingTemperatureFromOnline), nameof(HeatingTemperatureToOnline) });
			}
		}

		#endregion IValidatableObject implementation

		#region Statics

		public static string PrefixOfCode1c = "ДВ";
		public static int LengthOfCode1c = 10;
		private MasterServiceType? _masterServiceType;

		/// <summary>
		/// Категории товаров к которым применима категория продажи
		/// (доступность для продаж) "<see cref="SaleCategory"/>"
		/// </summary>
		/// <returns>Массив <see cref="NomenclatureCategory"/> к которым может применяться <see cref="SaleCategory"/></returns>
		public static NomenclatureCategory[] GetCategoriesWithSaleCategory()
		{
			return new[]
			{
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.bottle,
				NomenclatureCategory.spare_parts
			};
		}

		public static NomenclatureCategory[] GetCategoriesForShipment()
		{
			return new[]
			{
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.bottle,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.material
			};
		}

		public static NomenclatureCategory[] GetCategoriesForProductMaterial()
		{
			return new[] { NomenclatureCategory.material, NomenclatureCategory.bottle };
		}

		public static NomenclatureCategory[] GetCategoriesForSale()
		{
			return new[]
			{
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.service,
				NomenclatureCategory.material
			};
		}

		public static NomenclatureCategory[] GetCategoriesForSaleToOrder()
		{
			return new[]
			{
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.deposit,
				NomenclatureCategory.service,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.bottle,
				NomenclatureCategory.material
			};
		}

		/// <summary>
		/// Список номенклатур доступных для добавления в товары
		/// из диалога изменения заказа в закрытии МЛ
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForEditOrderFromRL()
		{
			return new[]
			{
				NomenclatureCategory.additional,
				NomenclatureCategory.water,
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.service,
				NomenclatureCategory.master
			};
		}

		public static NomenclatureCategory[] GetCategoriesForMaster()
		{
			return GetCategoriesForSale()
				.Concat(new []
				{
					NomenclatureCategory.master,
					NomenclatureCategory.spare_parts
				}).ToArray();
		}

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoods()
		{
			return new[]
			{
				NomenclatureCategory.bottle,
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.water,
				NomenclatureCategory.CashEquipment,
				NomenclatureCategory.Stationery,
				NomenclatureCategory.OfficeEquipment,
				NomenclatureCategory.PromotionalProducts,
				NomenclatureCategory.Overalls,
				NomenclatureCategory.HouseholdInventory,
				NomenclatureCategory.Tools,
				NomenclatureCategory.CarParts
			};
		}

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе без учёта 19л воды.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoodsWithoutEmptyBottles()
		{
			return new[]
			{
				NomenclatureCategory.water,
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.PromotionalProducts
			};
		}

		public static NomenclatureCategory[] GetCategoriesWithEditablePrice()
		{
			return new[]
			{
				NomenclatureCategory.bottle,
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.water,
				NomenclatureCategory.service,
				NomenclatureCategory.deposit,
				NomenclatureCategory.master
			};
		}

		public static NomenclatureCategory[] GetAllCategories()
		{
			return Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>().ToArray();
		}

		/// <summary>
		/// Определяет категории для которых необходимо создавать доп соглашение по продаже воды
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesRequirementForWaterAgreement()
		{
			return new[]
			{
				NomenclatureCategory.water
			};
		}

		public static NomenclatureCategory[] GetCategoriesNotNeededToLoad()
		{
			return new[]
			{
				NomenclatureCategory.service,
				NomenclatureCategory.deposit,
				NomenclatureCategory.master
			};
		}

		/// <summary>
		/// Категории, для которых обазательно должны быть заполнены вес и объём
		/// </summary>
		public static readonly NomenclatureCategory[] CategoriesWithWeightAndVolume =
		{
			NomenclatureCategory.water,
			NomenclatureCategory.equipment,
			NomenclatureCategory.additional,
			NomenclatureCategory.bottle
		};

		/// <summary>
		/// Категории для номенклатур с серийным номером
		/// </summary>
		public static readonly NomenclatureCategory[] CategoriesWithSerial =
		{
			NomenclatureCategory.equipment,
			NomenclatureCategory.Stationery,
			NomenclatureCategory.EquipmentForIndoorUse,
			NomenclatureCategory.OfficeEquipment,
			NomenclatureCategory.ProductionEquipment,
			NomenclatureCategory.Vehicle
		};

		#endregion Statics

		public virtual void ResetNotWaterOnlineParameters()
		{
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			PumpType = null;
			CupHolderBracingType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotKulerOnlineParameters()
		{
			IsSparklingWater = false;
			PumpType = null;
			CupHolderBracingType = null;
		}

		public virtual void ResetNotPurifierOnlineParameters()
		{
			ResetNotKulerOnlineParameters();
			EquipmentWorkloadType = null;
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotWaterPumpOnlineParameters()
		{
			IsSparklingWater = false;
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			CupHolderBracingType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotCupHolderOnlineParameters()
		{
			IsSparklingWater = false;
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			PumpType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetCoolingParameters()
		{
			NewCoolingPower = null;
			CoolingPowerUnits = null;
			CoolingProductivity = null;
			CoolingProductivityComparisionSign = null;
			CoolingProductivityUnits = null;
			NewCoolingType = null;
			CoolingTemperatureFromOnline = null;
			CoolingTemperatureToOnline = null;
		}

		public virtual void ResetHeatingParameters()
		{
			NewHeatingPower = null;
			HeatingPowerUnits = null;
			HeatingProductivity = null;
			HeatingProductivityComparisionSign = null;
			HeatingProductivityUnits = null;
			ProtectionOnHotWaterTap = null;
			HeatingTemperatureFromOnline = null;
			HeatingTemperatureToOnline = null;
		}

		public virtual void ResetLockerRefrigeratorVolume()
		{
			LockerRefrigeratorVolume = null;
		}
	}
}
