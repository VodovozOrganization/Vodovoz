using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Часть города",
		NominativePlural = "Части города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeoGroup : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }
		IList<GeoGroupVersion> _versions = new List<GeoGroupVersion>();
		GenericObservableList<GeoGroupVersion> _observableVersions;

		string _name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название части города должно быть заполнено")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value, () => Name);
		}

		[Display(Name = "Версии")]
		public virtual IList<GeoGroupVersion> Versions
		{
			get => _versions;
			set => SetField(ref _versions, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeoGroupVersion> ObservableVersions
		{
			get
			{
				if(_observableVersions == null)
					_observableVersions = new GenericObservableList<GeoGroupVersion>(Versions);
				return _observableVersions;
			}
		}


		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Необходимо указать имя", new[] { nameof(Name) });
			}
		}

		#endregion IValidatableObject implementation
	}
}
