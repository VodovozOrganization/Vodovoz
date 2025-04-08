using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Logistics
{
	/// <summary>
	/// График доставки
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "графики доставки",
		Nominative = "график доставки",
		Prepositional = "графике доставки",
		PrepositionalPlural = "графиках доставки",
		Accusative = "графике доставки",
		AccusativePlural = "графиках доставки",
		Genitive = "графика доставки",
		GenitivePlural = "графиков доставки")]
	[EntityPermission]
	[HistoryTrace]
	public class DeliveryScheduleEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}
	}
}
