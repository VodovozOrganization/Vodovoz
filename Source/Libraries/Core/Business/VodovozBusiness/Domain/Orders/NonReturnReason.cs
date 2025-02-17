using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины невозврата имущества",
		Nominative = "причина невозврата имущества")]
	[HistoryTrace]
	[EntityPermission]
	public class NonReturnReason : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private bool _needForfeit;
		public virtual int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Необходимо начислять неустойку
		/// </summary>
		[Display(Name = "Необходимо начислять неустойку")]
		public virtual bool NeedForfeit
		{
			get => _needForfeit;
			set => SetField(ref _needForfeit, value);
		}
	}
}
