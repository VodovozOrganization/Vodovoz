using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Store
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "спецификации продукции",
		Nominative = "спецификация продукции"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class ProductSpecification : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private Nomenclature _product;
		private IList<ProductSpecificationMaterial> _materials;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название спецификации должно быть заполнено.")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Продукция")]
		[Required(ErrorMessage = "Продукция должна быть указана.")]
		public virtual Nomenclature Product
		{
			get => _product;
			set => SetField(ref _product, value);
		}

		[Display(Name = "Материалы")]
		public virtual IList<ProductSpecificationMaterial> Materials
		{
			get => _materials;
			set => SetField(ref _materials, value);
		}

		#endregion

		public virtual string Title => $"Спецификация на <{Product.Name}>";

		public ProductSpecification()
		{
			Name = string.Empty;
		}
	}
}
