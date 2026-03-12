using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using VodovozInfrastructure.Versions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "Часть города",
		NominativePlural = "Части города",
		GenitivePlural = "Частей города")]
	[EntityPermission]
	[HistoryTrace]
	public class GeoGroup : GeoGroupEntity, IValidatableObject
	{
		private IList<GeoGroupVersion> _versions = new List<GeoGroupVersion>();
		private GenericObservableList<GeoGroupVersion> _observableVersions;

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
