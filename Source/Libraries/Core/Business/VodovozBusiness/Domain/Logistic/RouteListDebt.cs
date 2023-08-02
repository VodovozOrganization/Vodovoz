using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "долги по маршрутному листу",
		Nominative = "долг по маршрутному листу"
	)]
	[HistoryTrace]
	public class RouteListDebt : PropertyChangedBase, IDomainObject
	{
		private RouteList _routeList;
		private decimal _debt;

		public virtual int Id { get; set; }

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Долг по МЛ")]
		public virtual decimal Debt
		{
			get => _debt;
			set => SetField(ref _debt, value);
		}

		public virtual string Title => $"Долг по МЛ №{RouteList.Id}";
	}
}
