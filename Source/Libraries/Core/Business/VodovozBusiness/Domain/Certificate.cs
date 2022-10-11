using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сертификаты продукции",
		Nominative = "сертификат продукции")]
	[HistoryTrace]
	[EntityPermission]
	public class Certificate : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		CertificateType? typeOfCertificate;
		[Display(Name = "Тип")]
		public virtual CertificateType? TypeOfCertificate {
			get => typeOfCertificate;
			set => SetField(ref typeOfCertificate, value, () => TypeOfCertificate);
		}

		byte[] imageFile;
		[Display(Name = "Изображение")]
		public virtual byte[] ImageFile {
			get => imageFile;
			set => SetField(ref imageFile, value, () => ImageFile);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		DateTime? expirationDate;
		[Display(Name = "Дата окончания срока действия")]
		public virtual DateTime? ExpirationDate {
			get => expirationDate;
			set => SetField(ref expirationDate, value, () => ExpirationDate);
		}

		DateTime? startDate = DateTime.Today;
		[Display(Name = "Дата начала срока действия либо выдачи")]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		IList<Nomenclature> nomenclatures = new List<Nomenclature>();
		[Display(Name = "Отгружаемые номенклатуры")]
		public virtual IList<Nomenclature> Nomenclatures {
			get => nomenclatures;
			set => SetField(ref nomenclatures, value, () => Nomenclatures);
		}

		GenericObservableList<Nomenclature> observableNomenclatures;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableNomenclatures {
			get {
				if(observableNomenclatures == null)
					observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures);
				return observableNomenclatures;
			}
		}

		public Certificate() { }

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!StartDate.HasValue)
				yield return new ValidationResult("Дата выдачи (начала срока действия) сертификата должно быть указано.",
					new[] { this.GetPropertyName(o => o.StartDate) });

			if(StartDate.HasValue && ExpirationDate.HasValue && StartDate.Value > ExpirationDate.Value)
				yield return new ValidationResult("Дата окончания срока действия не может быть меньше даты выдачи (начала срока действия).",
					new[] {
						this.GetPropertyName(o => o.StartDate),
						this.GetPropertyName(o => o.ExpirationDate)
					}
				);

			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult("Название сертификата должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.Name) });

			if(!TypeOfCertificate.HasValue)
				yield return new ValidationResult("Тип сертификата должен быть указан.",
					new[] { this.GetPropertyName(o => o.TypeOfCertificate) });

			if(ImageFile == null)
				yield return new ValidationResult("Изображение сертификата должно быть загружено.",
					new[] { this.GetPropertyName(o => o.ImageFile) });
		}

		#endregion
	}

	/// <summary>
	/// Тип сертификата
	/// </summary>
	public enum CertificateType
	{
		[Display(Name = "Для ТМЦ")]
		Nomenclature
	}

	public class CertificateTypeStringType : NHibernate.Type.EnumStringType
	{
		public CertificateTypeStringType() : base(typeof(CertificateType)) { }
	}
}