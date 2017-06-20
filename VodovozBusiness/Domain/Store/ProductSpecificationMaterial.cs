using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Store
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
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

		[Min (1)]
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