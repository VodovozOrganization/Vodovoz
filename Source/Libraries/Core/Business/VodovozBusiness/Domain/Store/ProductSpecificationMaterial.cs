using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using RangeAttribute = Vodovoz.Attributes.RangeAttribute;

namespace Vodovoz.Domain.Store
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "материалы спецификации",
		Nominative = "материал спецификации"
	)]
	public class ProductSpecificationMaterial : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Nomenclature material;

		[Display(Name = "Материал")]
		public virtual Nomenclature Material {
			get { return material; }
			set { SetField (ref material, value, () => Material); }
		}

		decimal amount;

		[Range(typeof(decimal), "1", ErrorMessage = "Количество должно быть больше 1")]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount);
			}
		}

		ProductSpecification productSpec;

		[Display (Name = "ID спецификации")]
		public virtual ProductSpecification ProductSpec
		{
			get { return productSpec; }
			set { SetField(ref productSpec, value, () => ProductSpec); }
		}


		#endregion

		public virtual string NomenclatureName {
			get { return Material.Name;}
		}

		public virtual string Title{
			get{
				return String.Format("Материал <{0}> из спецификации на производства", Material.Name);
			}
		}

		public ProductSpecificationMaterial ()
		{
			
		}
	}
}
