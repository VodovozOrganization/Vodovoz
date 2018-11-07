using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Store
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "cпецификации продукции",
		Nominative = "cпецификация продукции"
	)]
	public class ProductSpecification : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[Required (ErrorMessage = "Название спецификации должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		Nomenclature product;

		[Display(Name = "Продукция")]
		[Required (ErrorMessage = "Продукция должна быть указана.")]
		public virtual Nomenclature Product {
			get { return product; }
			set { SetField (ref product, value, () => Product); }
		}

		IList<ProductSpecificationMaterial> materials;

		[Display(Name = "Материалы")]
		public virtual IList<ProductSpecificationMaterial> Materials {
			get { return materials; }
			set { SetField (ref materials, value, () => Materials); }
		}

		#endregion

		public virtual string Title{
			get{
				return String.Format("Спецификация на <{0}>", Product.Name);
			}
		}


		public ProductSpecification ()
		{
			Name = String.Empty;
		}
	}
}