using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "типы объекта",
		Nominative = "тип объекта"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class DeliveryPointCategory : PropertyChangedBase, IDomainObject
	{
		public DeliveryPointCategory() { }

		#region Свойства

		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название типа объекта")]
		[Required(ErrorMessage = "Название типа объекта должно быть указано")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		#endregion Свойства
	}
}
