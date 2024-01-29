using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "колонки в маршрутном листе",
		Nominative = "колонка маршрутного листа")]
	[HistoryTrace]
	public class RouteColumn : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private string _shortName;
		private bool _isHighlighted;
		public virtual int Id { get; set; }

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		[StringLength(20)]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Короткое название")]
		[StringLength(3, ErrorMessage = "Короткое название не должно быть длиннее 3-х символов")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		[Display(Name = "Ячейка выделена")]
		public virtual bool IsHighlighted
		{
			get => _isHighlighted;
			set => SetField(ref _isHighlighted, value);
		}

		public virtual string Title => Name;

		public RouteColumn ()
		{
			Name = string.Empty;
		}
	}
}

