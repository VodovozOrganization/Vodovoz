using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{

	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "фиксированные цены",
		Nominative = "фиксированная цена")]
	[HistoryTrace]
	public class WaterSalesAgreementFixedPrice : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Title {
			get {
				return $"{Nomenclature.Name} - {Price}";
			}
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		AdditionalAgreement additionalAgreement;

		[Display (Name = "Соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement{
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}

		public WaterSalesAgreementFixedPrice()
		{
		}

		public WaterSalesAgreementFixedPrice(Nomenclature nomenclature, decimal price)
		{
			this.nomenclature = nomenclature;
			this.price = price;
		}

	}
	
}
