using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Дни недели автозаказов с ИПЗ",
		Nominative = "День недели автозаказа с ИПЗ",
		Prepositional = "Дне недели автозаказа с ИПЗ",
		PrepositionalPlural = "Днях недели автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateWeekday : PropertyChangedBase, IDomainObject
	{
		private int _templateId;
		private WeekDayName _weekday;

		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		[Display(Name = "Идентификатор шаблона")]
		public virtual int TemplateId
		{
			get => _templateId;
			set => SetField(ref _templateId, value);
		}

		/// <summary>
		/// День недели
		/// </summary>
		[Display(Name = "День недели")]
		public virtual WeekDayName Weekday
		{
			get => _weekday;
			set => SetField(ref _weekday, value);
		}

		public static OnlineOrderTemplateWeekday Create(int templateId, WeekDayName weekday) =>
			new OnlineOrderTemplateWeekday
		{
			TemplateId = templateId,
			Weekday = weekday
		};
	}
}
