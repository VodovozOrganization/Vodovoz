using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Операции изменения свободных остатков МЛ",
		Nominative = "Операция изменения свободных остатков МЛ")]
	[HistoryTrace]
	public class DeliveryFreeBalanceOperation : OperationBase
	{
		private RouteList _routeList;
		private Nomenclature _nomenclature;
		private decimal _amount;

		[Display(Name = "Маршрутрный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Кол-во")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		public virtual string Title => $"Операция изменения свободных остатков МЛ №{RouteList.Id} {Nomenclature.Name} кол-во {Amount}";
	}
}
