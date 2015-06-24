using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Vodovoz.Domain
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

		int amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount);
			}
		}

		#endregion

		public string NomenclatureName {
			get { return Material.Name;}
		}

		public ProductSpecificationMaterial ()
		{
			
		}
	}
}