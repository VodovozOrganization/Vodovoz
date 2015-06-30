using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QSProjectsLib;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Строки заказа", ObjectName = "строка заказа")]
	public class OrderItem: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		AdditionalAgreement additionalAgreement;

		[Display (Name = "Дополнительное соглашения")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		MeasurementUnits units;

		[Display (Name = "Единица изменения")]
		public virtual MeasurementUnits Units {
			get { return units; }
			set { SetField (ref units, value, () => Units); }
		}

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		int count;

		[Display (Name = "Количество")]
		public virtual int Count {
			get { return count; }
			set { SetField (ref count, value, () => Count); }
		}

		public virtual string NomenclatureString {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string PriceString {
			get { return CurrencyWorks.GetShortCurrencyString (Price); }
		}

		public virtual string CountString {
			get { return String.Format ("{0} шт.", Count.ToString ()); }
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}
}

