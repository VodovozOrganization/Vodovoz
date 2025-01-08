using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "топливо",
		Nominative = "топливо"
	)]
	public class FuelProduct : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _fuelTypeId;
		private string _productId;
		private string _description;
		private bool _isArchived;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Код типа топлива")]
		public virtual int FuelTypeId
		{
			get => _fuelTypeId;
			set => SetField(ref _fuelTypeId, value);
		}

		[Display(Name = "Код топлива")]
		public virtual string ProductId
		{
			get => _productId;
			set => SetField(ref _productId, value);
		}

		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "В архиве")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}
	}
}
