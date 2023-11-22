using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		GenitivePlural = "дополнительных условий МЛ",
		NominativePlural = "дополнительные условия МЛ",
		Nominative = "дополнительное условие МЛ")]
	[HistoryTrace]
	public class RouteListSpecialCondition : PropertyChangedBase, IDomainObject
	{
		private int _routeListSpecialConditionTypeId;
		private int _routeListId;
		private bool _accepted;
		private DateTime _createdAt;

		public virtual int Id { get; set; }

		[Display(Name = "Номер маршрутного листа")]
		[HistoryIdentifier(TargetType = typeof(RouteList))]
		public virtual int RouteListId
		{
			get => _routeListId;
			set => SetField(ref _routeListId, value);
		}

		[Display(Name = "Тип дополнительного условия")]
		[HistoryIdentifier(TargetType = typeof(RouteListSpecialConditionType))]
		public virtual int RouteListSpecialConditionTypeId
		{
			get => _routeListSpecialConditionTypeId;
			set => SetField(ref _routeListSpecialConditionTypeId, value);
		}

		[Display(Name = "Принято")]
		public virtual bool Accepted
		{
			get => _accepted;
			set => SetField(ref _accepted, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}
	}
}
