using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Project.Repositories;
using QS.Utilities.Text;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Suppliers
{
	[Appellative(Gender = GrammaticalGender.Masculine,
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

		SupplierOrderingType suppliersOrdering;
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

		IList<Nomenclature> requestingNomenclatures = new List<Nomenclature>();
		[Display(Name = "Запрашиваемые ТМЦ")]
		public virtual IList<Nomenclature> RequestingNomenclatures {
			get => requestingNomenclatures;
			set => SetField(ref requestingNomenclatures, value);
		}

		GenericObservableList<Nomenclature> observableRequestingNomenclatures;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableRequestingNomenclatures {
			get {
				if(observableRequestingNomenclatures == null)
					observableRequestingNomenclatures = new GenericObservableList<Nomenclature>(RequestingNomenclatures);
				return observableRequestingNomenclatures;
			}
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

		#endregion вычисляемые

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Необходимо заполнить название",
					new[] { this.GetPropertyName(o => o.Name) }
				);
		}
	}

	public enum SupplierOrderingType
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Самый дешёвый")]
		TheCheapest,
		[Display(Name = "ТОП-3")]
		Top3
	}

	public class SupplierOrderingTypeStringType : EnumStringType
	{
		public SupplierOrderingTypeStringType() : base(typeof(SupplierOrderingType)) { }
	}
}