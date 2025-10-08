using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	/// <summary>
	/// Категория штрафов
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Accusative = "категорию штрафов",
		AccusativePlural = "категории штрафов",
		Genitive = "категории штрафов",
		GenitivePlural = "категорий штрафов",
		Nominative = "категория штрафов",
		NominativePlural = "категории штрафов",
		Prepositional = "категории штрафов",
		PrepositionalPlural = "категориях штрафов")]
	[EntityPermission]
	[HistoryTrace]
	public class FineCategoryEntity : PropertyChangedBase, INamedDomainObject, IBusinessObject
	{
		private int _id;
		private string _name;
		private bool _isArchive;

		/// <summary>
		/// Идентификатор
		/// Код номенклатуры
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Архивная
		/// </summary>
		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual IUnitOfWork UoW { set; get; }
	}
}
