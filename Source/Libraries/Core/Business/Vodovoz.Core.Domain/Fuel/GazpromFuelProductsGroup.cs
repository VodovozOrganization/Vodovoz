using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Fuel
{
	/// <summary>
	/// Группа продуктов топлива Газпрома
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Группы продуктов топлива Газпрома",
		Nominative = "Группа продуктов топлива Газпрома"
	)]
	public class GazpromFuelProductsGroup : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _fuelTypeId;
		private string _gazpromFuelProductGroupId;
		private string _gazpromFuelProductGroupName;
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
		/// Код типа топлива в ДВ
		/// </summary>
		[Display(Name = "Код типа топлива в ДВ")]
		public virtual int FuelTypeId
		{
			get => _fuelTypeId;
			set => SetField(ref _fuelTypeId, value);
		}

		/// <summary>
		/// Код группы топлива в газпроме
		/// </summary>
		[Display(Name = "Код группы топлива в газпроме")]
		public virtual string GazpromFuelProductGroupId
		{
			get => _gazpromFuelProductGroupId;
			set => SetField(ref _gazpromFuelProductGroupId, value);
		}

		/// <summary>
		/// Название группы топлива в газпроме
		/// </summary>
		[Display(Name = "Код группы топлива в газпроме")]
		public virtual string GazpromFuelProductGroupName
		{
			get => _gazpromFuelProductGroupName;
			set => SetField(ref _gazpromFuelProductGroupName, value);
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
