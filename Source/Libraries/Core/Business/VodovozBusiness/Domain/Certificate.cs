using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сертификаты продукции",
		Nominative = "сертификат продукции")]
	[HistoryTrace]
	[EntityPermission]
	public class Certificate : CertificateEntity, IValidatableObject
	{
		private IList<Nomenclature> _nomenclatures = new List<Nomenclature>();
		private GenericObservableList<Nomenclature> _observableNomenclatures;

		[Display(Name = "Отгружаемые номенклатуры")]
		public virtual new IList<Nomenclature> Nomenclatures {
			get => _nomenclatures;
			set => SetField(ref _nomenclatures, value, () => Nomenclatures);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual new GenericObservableList<Nomenclature> ObservableNomenclatures {
			get {
				if(_observableNomenclatures == null)
				{
					_observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures);
				}

				return _observableNomenclatures;
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!StartDate.HasValue)
			{
				yield return new ValidationResult("Дата выдачи (начала срока действия) сертификата должно быть указано.",
					new[] { this.GetPropertyName(o => o.StartDate) });
			}

			if(StartDate.HasValue && ExpirationDate.HasValue && StartDate.Value > ExpirationDate.Value)
			{
				yield return new ValidationResult("Дата окончания срока действия не может быть меньше даты выдачи (начала срока действия).",
					new[] {
						this.GetPropertyName(o => o.StartDate),
						this.GetPropertyName(o => o.ExpirationDate)
					}
				);
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название сертификата должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.Name) });
			}

			if(!TypeOfCertificate.HasValue)
			{
				yield return new ValidationResult("Тип сертификата должен быть указан.",
					new[] { this.GetPropertyName(o => o.TypeOfCertificate) });
			}

			if(ImageFile == null)
			{
				yield return new ValidationResult("Изображение сертификата должно быть загружено.",
					new[] { this.GetPropertyName(o => o.ImageFile) });
			}
		}

		#endregion
	}
}
