using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

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
		private ComplaintObject _complaintObject;
		private IList<Subdivision> _subdivisions = new List<Subdivision>();
		private GenericObservableList<Subdivision> _observableSubdivisions;

		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название вида")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		[Display(Name = "Объект рекламаций")]
		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set => SetField(ref _complaintObject, value);
		}

		public virtual string GetFullName => !IsArchive ? Name : string.Format("(Архив) {0}", Name);

		public virtual string Title => string.Format("Вид рекламации №{0} ({1})", Id, Name);

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Укажите название вида рекламации",
					new[] { this.GetPropertyName(o => o.Name) }
				);

			if(Name?.Length > 100)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
		}

		[Display(Name = "Подразделения")]
		public virtual IList<Subdivision> Subdivisions
		{
			get => _subdivisions;
			set => SetField(ref _subdivisions, value);
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