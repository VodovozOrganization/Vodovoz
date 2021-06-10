using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "виды событий ТС",
		Nominative = "вид события ТС")]
	[EntityPermission]
	[HistoryTrace]

	public class CarEventType : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private string _shortName;
		private bool _needComment;
		private bool _isArchive;

		#region Свойства

		public virtual int Id { get; set; }

		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Required(ErrorMessage = "Сокращённое название должно быть заполнено.")]
		[Display(Name = "Сокращенное название")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		[Display(Name = "Обязательность комментария")]
		public virtual bool NeedComment
		{
			get => _needComment;
			set => SetField(ref _needComment, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		#endregion
	}
}
