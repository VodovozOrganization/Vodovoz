using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "вид рекламации",
		Nominative = "вид рекламации",
		Prepositional = "виде рекламации",
		PrepositionalPlural = "видах рекламаций"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintKind : BusinessObjectBase<Complaint>, IDomainObject, IValidatableObject
	{
		private string _name;
		private ComplaintObject _complaintObject;
		private bool _isArchive;
		private IList<Subdivision> _subdivisions = new List<Subdivision>();
		private GenericObservableList<Subdivision> _observableSubdivisions;

		public virtual int Id { get; set; }

		[Display(Name = "Название вида")]
		public virtual string Name {
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Объект рекламаций")]
		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set => SetField(ref _complaintObject, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual string GetFullName => !IsArchive ? Name : $"(Архив) {Name}";

		public virtual string Title => Name;

		[Display(Name = "Подразделения")]
		public virtual IList<Subdivision> Subdivisions
		{
			get => _subdivisions;
			set => SetField(ref _subdivisions, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Укажите название вида рекламации",
					new[] { nameof(Name) });
			}

			if(Name?.Length > 100)
			{
				yield return new ValidationResult(
					$"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Subdivision> ObservableSubdivisions => 
			_observableSubdivisions ?? (_observableSubdivisions = new GenericObservableList<Subdivision>(Subdivisions));

		public virtual void AddSubdivision(Subdivision subdivision)
		{
			if(ObservableSubdivisions.Contains(subdivision))
			{
				return;
			}
			
			ObservableSubdivisions.Add(subdivision);
		}

		public virtual void RemoveSubdivision(Subdivision subdivision)
		{
			if(ObservableSubdivisions.Contains(subdivision))
			{
				ObservableSubdivisions.Remove(subdivision);
			}
		}
	}
}
