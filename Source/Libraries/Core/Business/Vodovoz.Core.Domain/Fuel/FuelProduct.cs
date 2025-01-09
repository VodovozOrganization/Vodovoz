using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Fuel
{
	/// <summary>
	/// Топливо
	/// </summary>
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

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Код типа топлива
		/// </summary>
		[Display(Name = "Код типа топлива")]
		public virtual int FuelTypeId
		{
			get => _fuelTypeId;
			set => SetField(ref _fuelTypeId, value);
		}

		/// <summary>
		/// Код топлива в газпроме
		/// </summary>
		[Display(Name = "Код топлива в газпроме")]
		public virtual string ProductId
		{
			get => _productId;
			set => SetField(ref _productId, value);
		}

		/// <summary>
		/// Описание
		/// </summary>
		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		/// <summary>
		/// В архиве
		/// </summary>
		[Display(Name = "В архиве")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}
	}
}
