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
	public class GazpromFuelProduct : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _gazpromProductsGroupId;
		private string _gazpromFuelProductId;
		private string _gazpromFuelProductName;
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
		/// Код группы продуктов в газпроме
		/// </summary>
		[Display(Name = "Код группы продуктов в газпроме")]
		public virtual int GazpromProductsGroupId
		{
			get => _gazpromProductsGroupId;
			set => SetField(ref _gazpromProductsGroupId, value);
		}

		/// <summary>
		/// Код топлива в газпроме
		/// </summary>
		[Display(Name = "Код топлива в газпроме")]
		public virtual string GazpromFuelProductId
		{
			get => _gazpromFuelProductId;
			set => SetField(ref _gazpromFuelProductId, value);
		}

		/// <summary>
		/// Наименование топлива в газпроме
		/// </summary>
		[Display(Name = "Наименование топлива в газпроме")]
		public virtual string GazpromFuelProductName
		{
			get => _gazpromFuelProductName;
			set => SetField(ref _gazpromFuelProductName, value);
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
