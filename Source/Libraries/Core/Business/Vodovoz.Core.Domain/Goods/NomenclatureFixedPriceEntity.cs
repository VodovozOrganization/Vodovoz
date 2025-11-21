using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "фиксированные цены",
		Nominative = "фиксированная цена")]
	[HistoryTrace]
	public class NomenclatureFixedPriceEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private CounterpartyEntity _counterparty;
		private DeliveryPointEntity _deliveryPoint;
		private NomenclatureEntity _nomenclature;
		private decimal _price;
		private int _minCount;
		private bool _isEmployeeFixedPrice;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Точка доставки
		/// </summary>
		[Display(Name = "Точка доставки")]
		public virtual DeliveryPointEntity DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Цена
		/// </summary>
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		/// <summary>
		/// Минимальное количество
		/// </summary>
		[Display(Name = "Минимальное количество")]
		public virtual int MinCount
		{
			get => _minCount;
			set => SetField(ref _minCount, value);
		}

		/// <summary>
		/// Фикса сотрудника
		/// </summary>
		[Display(Name = "Фикса сотрудника")]
		public virtual bool IsEmployeeFixedPrice
		{
			get => _isEmployeeFixedPrice;
			set => SetField(ref _isEmployeeFixedPrice, value);
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title
		{
			get
			{
				if(Counterparty != null)
				{
					return $"Фикса клиента №{Counterparty.Id} {Counterparty.Name}";
				}

				return DeliveryPoint != null ? $"Фикса точки доставки №{DeliveryPoint.Id} {DeliveryPoint.CompiledAddress}" : $"Фикса №{Id}";
			}
		}

		/// <summary>
		/// Создать фиксированную цену для сотрудника
		/// </summary>
		/// <param name="namedDomainObject"></param>
		/// <returns></returns>
		public static NomenclatureFixedPriceEntity CreateEmployeeFixedPrice(INamedDomainObject namedDomainObject)
		{
			return new NomenclatureFixedPriceEntity
			{
				Nomenclature = new NomenclatureEntity
				{
					Id = namedDomainObject.Id,
					Name = namedDomainObject.Name
				},
				IsEmployeeFixedPrice = true
			};
		}
	}
}
