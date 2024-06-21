using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Store
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "материалы спецификации",
		Nominative = "материал спецификации"
	)]
	public class ProductSpecificationMaterial : PropertyChangedBase, IDomainObject, IValidatableObject
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
				return $"Материал <{Material.Name}> из спецификации на производства";
			}
		}

		public ProductSpecificationMaterial ()
		{
			
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Amount < 1)
			{
				yield return new ValidationResult("Количество должно быть больше 1", new[] { nameof(Amount) });
			}
		}
	}
}
