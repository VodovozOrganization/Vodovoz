using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение",
		GenitivePlural = "подразделений")]
	[EntityPermission]
	[HistoryTrace]
	public class Subdivision : SubdivisionEntity, IValidatableObject, IArchivable
	{
		private SalesPlan _defaultSalesPlan;
		private Employee _chief;
		private Subdivision _parentSubdivision;
		private IList<Subdivision> _childSubdivisions = new List<Subdivision>();
		private GenericObservableList<Subdivision> _observableChildSubdivisions;
		private IList<TypeOfEntity> _documentTypes = new List<TypeOfEntity>();
		private GenericObservableList<TypeOfEntity> _observableDocumentTypes;
		private GeoGroup _geographicGroup;
		private SubdivisionType _subdivisionType;

		#region Свойства

		[IgnoreHistoryTrace]
		[Display(Name = "Начальник подразделения")]
		public virtual Employee Chief
		{
			get => _chief;
			set
			{
				if(SetField(ref _chief, value))
				{
					ChiefId = value?.Id;
				}
			}
		}

		[Display(Name = "Вышестоящее подразделение")]
		public virtual Subdivision ParentSubdivision
		{
			get => _parentSubdivision;
			set => SetField(ref _parentSubdivision, value);
		}

		[Display(Name = "Дочерние подразделения")]
		public virtual IList<Subdivision> ChildSubdivisions
		{
			get => _childSubdivisions;
			set => SetField(ref _childSubdivisions, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Subdivision> ObservableChildSubdivisions
		{
			get
			{
				if(_observableChildSubdivisions == null)
				{
					_observableChildSubdivisions = new GenericObservableList<Subdivision>(ChildSubdivisions);
				}

				return _observableChildSubdivisions;
			}
		}

		[Display(Name = "Документы используемые в подразделении")]
		public virtual IList<TypeOfEntity> DocumentTypes
		{
			get => _documentTypes;
			set => SetField(ref _documentTypes, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TypeOfEntity> ObservableDocumentTypes
		{
			get
			{
				if(_observableDocumentTypes == null)
				{
					_observableDocumentTypes = new GenericObservableList<TypeOfEntity>(DocumentTypes);
				}

				return _observableDocumentTypes;
			}
		}

		[Display(Name = "Обслуживаемая часть города")]
		public virtual new GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}

		[Display(Name = "Тип подразделения")]
		public virtual SubdivisionType SubdivisionType
		{
			get => _subdivisionType;
			set => SetField(ref _subdivisionType, value);
		}

		public virtual SalesPlan DefaultSalesPlan
		{
			get => _defaultSalesPlan;
			set => SetField(ref _defaultSalesPlan, value);
		}

		#endregion

		#region Геттеры и методы

		/// <summary>
		/// Уровень в иерархии
		/// </summary>
		public virtual int GetLevel => ParentSubdivision == null ? 0 : ParentSubdivision.GetLevel + 1;

		public virtual bool HasChildSubdivisions => ChildSubdivisions.Any();

		/// <summary>
		/// Является ли подразделение ребёнком другого подразделения?
		/// </summary>
		/// <returns><c>true</c>, если является, <c>false</c> если не является.</returns>
		/// <param name="subdivision">Предпологаемый родитель.</param>
		public virtual bool IsChildOf(Subdivision subdivision)
		{
			if(this == subdivision)
			{
				return false;
			}

			Subdivision parent = ParentSubdivision;

			while(parent != null)
			{
				if(parent == subdivision)
				{
					return true;
				}

				parent = parent.ParentSubdivision;
			}

			return false;
		}

		public virtual string GetWarehousesNames(IUnitOfWork uow, ISubdivisionRepository subdivisionRepository)
		{
			var result = string.Empty;

			if(Id != 0)
			{
				var whs = subdivisionRepository.GetWarehouses(uow, this).Select(w => w.Name);
				result = string.Join(", ", whs);
			}

			return result;
		}

		public virtual GeoGroup GetGeographicGroup()
		{
			if(GeographicGroup != null)
			{
				return GeographicGroup;
			}

			if(ParentSubdivision == null)
			{
				return null;
			}

			return ParentSubdivision.GetGeographicGroup();
		}

		public virtual void SetChildsGeographicGroup(GeoGroup geographicGroup)
		{
			if(ParentSubdivision == null && !ChildSubdivisions.Any())
			{
				return;
			}

			foreach(var s in ChildSubdivisions)
			{
				s.GeographicGroup = GeographicGroup;
			}
		}

		public virtual void AddDocumentType(TypeOfEntity typeOfEntity)
		{
			if(ObservableDocumentTypes.Contains(typeOfEntity))
			{
				return;
			}

			ObservableDocumentTypes.Add(typeOfEntity);
		}

		public virtual void DeleteDocumentType(TypeOfEntity typeOfEntity)
		{
			if(ObservableDocumentTypes.Contains(typeOfEntity))
			{
				ObservableDocumentTypes.Remove(typeOfEntity);
			}
		}

		#endregion

		public virtual bool IsCashSubdivision => DocumentTypes
			.Any(x => x.Type == nameof(Income)
				|| x.Type == nameof(Expense)
				|| x.Type == nameof(AdvanceReport));

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название подразделения должно быть заполнено.",
					new[] { this.GetPropertyName(o => o.Name) });
			}

			if(ParentSubdivision != null && ParentSubdivision.IsChildOf(this))
			{
				yield return new ValidationResult(
					"Нельзя указывать 'Дочернее подразделение' в качестве родительского.",
					new[] { this.GetPropertyName(o => o.ParentSubdivision) }
				);
			}

			if(ShortName == null)
			{
				yield return new ValidationResult(
					"Укажите сокращённое название отдела.",
					new[] { this.GetPropertyName(o => o.ShortName) }
				);
			}

			if(ShortName?.Length > 20)
			{
				yield return new ValidationResult("Сокращенное наименование не может превышать 20 символов");
			}
		}

		#endregion
	}
}
