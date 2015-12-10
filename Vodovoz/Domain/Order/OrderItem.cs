using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки заказа",
		Nominative = "строка заказа")]
	public class OrderItem: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}


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

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		int count=-1;

		[Display (Name = "Количество")]
		public virtual int Count {
			get { return count; }
			set { 			
				if (count != -1) {	
					var oldDefaultPrice = DefaultPrice;
					var newDefaultPrice = GetDefaultPrice (value);
					if (Price == oldDefaultPrice)
						Price = newDefaultPrice;
					DefaultPrice = newDefaultPrice;
				}
				SetField (ref count, value, () => Count); 
			}
		}

		protected Decimal GetDefaultPrice(int count){			
			Decimal result=0;
			if (Nomenclature.Category == NomenclatureCategory.water) {
				result = Nomenclature.GetPrice (count);
				var waterSalesAgreement = AdditionalAgreement as WaterSalesAgreement;
				if (waterSalesAgreement.IsFixedPrice)
					result = waterSalesAgreement.FixedPrice;
			}
			return result;
		}
			
		Decimal defaultPrice=-1;

		public virtual Decimal DefaultPrice {
			get { 
				if (defaultPrice == -1) {
					defaultPrice = GetDefaultPrice (count);
				}
				return defaultPrice; 
			}
			set { SetField (ref defaultPrice, value, () => DefaultPrice); }
		}

		public virtual bool HasUserSpecifiedPrice(){
			return price != DefaultPrice;
		}


		public virtual bool CanEditAmount {
			get { return AdditionalAgreement == null || AdditionalAgreement.Type == AgreementType.WaterSales; }
		}

		public virtual string NomenclatureString {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string AgreementString {
			get { return AdditionalAgreement == null ? String.Empty : String.Format ("{0} №{1}", AdditionalAgreement.AgreementTypeTitle, AdditionalAgreement.AgreementNumber); }
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}
}

