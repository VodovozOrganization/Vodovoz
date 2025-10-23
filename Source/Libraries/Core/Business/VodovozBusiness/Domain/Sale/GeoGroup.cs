using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using VodovozInfrastructure.Versions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Часть города",
		NominativePlural = "Части города",
		GenitivePlural = "Частей города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeoGroup : PropertyChangedBase, IDomainObject, IValidatableObject, INamed
	{
		private string _name;
		private bool _isArchived;
		private IList<GeoGroupVersion> _versions = new List<GeoGroupVersion>();
		private GenericObservableList<GeoGroupVersion> _observableVersions;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название части города должно быть заполнено")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value, () => Name);
		}

		[Display(Name = "Версии")]
		public virtual IList<GeoGroupVersion> Versions
		{
			get => _versions;
			set => SetField(ref _versions, value);
		}

		[Display(Name = "В архиве")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
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

		public virtual GeoGroupVersion GetActualVersionOrNull()
		{
			var activeVersion = Versions.FirstOrDefault(x => x.Status == VersionStatus.Active);
			return activeVersion;
		}

		public virtual GeoGroupVersion GetVersionOrNull(DateTime date)
		{
			var activeVersion = Versions.Where(x => x.ActivationDate <= date)
				.Where(x => x.ClosingDate == null || x.ClosingDate.Value >= date)
				.OrderByDescending(x => x.ActivationDate)
				.FirstOrDefault();
			return activeVersion;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Необходимо указать имя", new[] { nameof(Name) });
			}

			foreach(var version in Versions)
			{
				foreach(var result in version.Validate(validationContext))
				{
					yield return result;
				}
			}
		}

		#endregion IValidatableObject implementation
	}
}
