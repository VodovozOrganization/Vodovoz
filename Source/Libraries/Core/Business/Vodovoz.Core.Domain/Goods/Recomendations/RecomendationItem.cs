using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods.Recomendations
{
	/// <summary>
	/// Строка рекомендаций
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative ="строка рекомендаций",
		AccusativePlural = "строки рекомендаций",
		Genitive = "строки рекомендаций",
		GenitivePlural = "строк рекомендаций",
		Nominative = "строка рекомендации",
		NominativePlural = "строки рекомендаций",
		Prepositional = "строке рекомендации",
		PrepositionalPlural = "строках рекомендаций")]
	[HistoryTrace]
	public class RecomendationItem : PropertyChangedBase, IDomainObject
	{
		private int _nomenclatureId;
		private int _id;
		private int _priority;
		private int _recomendationId;

		/// <summary>
		/// Конструктор по умолчанию для NHibernate
		/// </summary>
		protected RecomendationItem()
		{
		}

		/// <summary>
		/// Конструктор для создания новой строки рекомендаций
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		/// <param name="priority"></param>
		private RecomendationItem(int recomendationId, int nomenclatureId, int priority)
		{
			RecomendationId = recomendationId;
			NomenclatureId = nomenclatureId;
			Priority = priority;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			protected internal set => SetField(ref _id, value);
		}

		[Display(Name = "Идентификатор рекомендации")]
		[HistoryIdentifier(TargetType = typeof(Recomendation))]
		public virtual int RecomendationId
		{
			get => _recomendationId;
			protected internal set => SetField(ref _recomendationId, value);
		}

		[Display(Name = "Идентификатор номенклатуры")]
		[HistoryIdentifier(TargetType = typeof(NomenclatureEntity))]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			protected internal set => SetField(ref _nomenclatureId, value);
		}

		[Display(Name = "Идентификатор приоритет")]
		public virtual int Priority
		{
			get => _priority;
			protected internal set => SetField(ref _priority, value);
		}

		/// <summary>
		/// Создание строки рекомендации
		/// </summary>
		/// <param name="recomendationId"></param>
		/// <param name="nomenclatureId"></param>
		/// <param name="priority"></param>
		/// <returns></returns>
		internal static RecomendationItem Create(int recomendationId, int nomenclatureId, int priority)
		{
			return new RecomendationItem(recomendationId, nomenclatureId, priority);
		}
	}
}
