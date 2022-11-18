using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Suppliers;

namespace Vodovoz.Domain.Suppliers
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки поставщику",
		Nominative = "заявка поставщику"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class RequestToSupplier : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region свойства для маппинга

		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		SupplierOrderingType suppliersOrdering = SupplierOrderingType.Top3;
		[Display(Name = "Режим отображения поставщиков")]
		public virtual SupplierOrderingType SuppliersOrdering {
			get => suppliersOrdering;
			set => SetField(ref suppliersOrdering, value, () => SuppliersOrdering);
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		DateTime creatingDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreatingDate {
			get => creatingDate;
			set => SetField(ref creatingDate, value);
		}

		Employee creator;
		[Display(Name = "Автор заявки")]
		public virtual Employee Creator {
			get => creator;
			set => SetField(ref creator, value);
		}

		IList<RequestToSupplierItem> requestingNomenclatureItems = new List<RequestToSupplierItem>();
		[Display(Name = "Запрашиваемые ТМЦ")]
		public virtual IList<RequestToSupplierItem> RequestingNomenclatureItems {
			get => requestingNomenclatureItems;
			set => SetField(ref requestingNomenclatureItems, value);
		}

		GenericObservableList<RequestToSupplierItem> observableRequestingNomenclatureItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RequestToSupplierItem> ObservableRequestingNomenclatureItems {
			get {
				if(observableRequestingNomenclatureItems == null)
					observableRequestingNomenclatureItems = new GenericObservableList<RequestToSupplierItem>(RequestingNomenclatureItems);
				return observableRequestingNomenclatureItems;
			}
		}

		RequestStatus status;
		[Display(Name = "Статус заявки")]
		public virtual RequestStatus Status {
			get => status;
			set => SetField(ref status, value);
		}

		bool withDelayOnly;
		[Display(Name = "Только с отсрочкой")]
		public virtual bool WithDelayOnly {
			get => withDelayOnly;
			set => SetField(ref withDelayOnly, value);
		}

		#endregion свойства для маппинга


		#region вычисляемые

		public virtual string Title {
			get {
				return string.Format(
					"{0} №{1}",
					TypeOfEntityRepository.GetRealName(GetType())?.StringToTitleCase(),
					Id
				);
			}
		}

		IList<ILevelingRequestNode> levelingRequestNodes = new List<ILevelingRequestNode>();
		public virtual IList<ILevelingRequestNode> LevelingRequestNodes {
			get => levelingRequestNodes;
			set => SetField(ref levelingRequestNodes, value);
		}

		GenericObservableList<ILevelingRequestNode> observableLevelingRequestNodes;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ILevelingRequestNode> ObservableLevelingRequestNodes {
			get {
				if(observableLevelingRequestNodes == null)
					observableLevelingRequestNodes = new GenericObservableList<ILevelingRequestNode>(LevelingRequestNodes);
				return observableLevelingRequestNodes;
			}
		}

		public virtual decimal MinimalTotalSum {
			get {
				decimal sum = 0m;
				foreach(ILevelingRequestNode nom in ObservableLevelingRequestNodes) {
					if(nom.Children != null && nom.Children.Any()) {
						//берём первого ребёнка, т.к. они сортируются в порядке возрастания цены
						sum += nom.Children[0].SupplierPriceItem.Price * nom.Quantity;
					}
				}
				return sum;
			}
		}

		#endregion вычисляемые

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Необходимо заполнить название",
					new[] { this.GetPropertyName(o => o.Name) }
				);

			if(RequestingNomenclatureItems == null || !RequestingNomenclatureItems.Any())
				yield return new ValidationResult(
					"Список запрашиваемых ТМЦ пуст",
					new[] { this.GetPropertyName(o => o.RequestingNomenclatureItems) }
				);

			#region валидация строк заявки

			var allValidationResultsOfItems = RequestingNomenclatureItems.SelectMany(x => x.Validate(validationContext));
			foreach(var result in allValidationResultsOfItems)
				yield return result;

			#endregion валидация строк заявки
		}

		#region Methods

		public virtual void RequestingNomenclaturesListRefresh(IUnitOfWork uow, ISupplierPriceItemsRepository supplierPriceItemsRepository, SupplierOrderingType orderingType)
		{
			ObservableLevelingRequestNodes.Clear();
			foreach(var reqItem in RequestingNomenclatureItems.Where(i => !i.Transfered)) {
				var price = reqItem.Nomenclature.NomenclaturePrice.OrderBy(p => p.Price).FirstOrDefault();
				if(price != null)
					uow.Session.Refresh(price);

				reqItem.Parent = null;
				reqItem.Children = new List<ILevelingRequestNode>();

				var children = supplierPriceItemsRepository.GetSupplierPriceItemsForNomenclature(
					uow,
					reqItem.Nomenclature,
					orderingType,
					new[] { AvailabilityForSale.Available },
					WithDelayOnly
				);
				foreach(var child in children) {
					uow.Session.Refresh(child);
					uow.Session.Refresh(child.Supplier);
					reqItem.Children.Add(
						new SupplierNode {
							Parent = reqItem,
							SupplierPriceItem = child
						}
					);
				}
				ObservableLevelingRequestNodes.Add(reqItem);
			}
		}

		public virtual void RemoveNomenclatureRequest(int nomenclatureId)
		{
			var removableItems = new List<RequestToSupplierItem>(
				ObservableRequestingNomenclatureItems.Where(i => i.Nomenclature.Id == nomenclatureId).ToList()
			);

			foreach(var item in removableItems) {
				ObservableRequestingNomenclatureItems.Remove(item);
			}
		}

		#endregion Methods
	}

	public enum SupplierOrderingType
	{
		[Display(Name = "Самый дешёвый")]
		TheCheapest,
		[Display(Name = "ТОП-3")]
		Top3,
		[Display(Name = "Все")]
		All
	}

	public class SupplierOrderingTypeStringType : EnumStringType
	{
		public SupplierOrderingTypeStringType() : base(typeof(SupplierOrderingType)) { }
	}

	public enum RequestStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "Закрыта")]
		Closed
	}

	public class RequestStatusStringType : EnumStringType
	{
		public RequestStatusStringType() : base(typeof(RequestStatus)) { }
	}

	public class SupplierNode : ILevelingRequestNode
	{
		public int Id { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public decimal Quantity { get; set; }
		public RequestToSupplier RequestToSupplier { get; set; }

		public SupplierPriceItem SupplierPriceItem { get; set; }
		public ILevelingRequestNode Parent { get; set; }
		public IList<ILevelingRequestNode> Children { get; set; }
	}
}