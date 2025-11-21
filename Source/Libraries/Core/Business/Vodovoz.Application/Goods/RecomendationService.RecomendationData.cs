using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods.Recomendations;

namespace Vodovoz.Application.Goods
{
	internal sealed partial class RecomendationService
	{
		/// <summary>
		/// Представляет набор рекомендаций
		/// </summary>
		public class RecomendationData
		{
			/// <summary>
			/// Общая рекомендация, для всех типов персон и КА
			/// </summary>
			public Recomendation CommonRecomendation { get; set; }
			/// <summary>
			/// Рекомендация, соответствующая типу помещения и типу КА
			/// </summary>
			public Recomendation SpecifiedRecomendation { get; set; }
			/// <summary>
			/// Рекомендация, соответствующая типу помещения, общая для всех типов КА
			/// </summary>
			public Recomendation RoomTypeRecomendation { get; set; }
			/// <summary>
			/// Рекомендация, соответствующая типу КА, общая для всех типов помещений
			/// </summary>
			public Recomendation PersonTypeRecomendation { get; set; }

			/// <summary>
			/// Строки общей рекомендации, отсортированные по приоритету
			/// </summary>
			public IEnumerable<RecomendationItem> CommonRecomendationItems =>
				CommonRecomendation?.Items?
				.OrderBy(x => x.Priority)
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();

			/// <summary>
			/// Строки рекомендации, соответствующей типу помещения и типу КА, отсортированные по приоритету
			/// </summary>
			public IEnumerable<RecomendationItem> SpecifiedRecomendationItems =>
				SpecifiedRecomendation?.Items?
				.OrderBy(x => x.Priority)
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();

			/// <summary>
			/// Строки рекомендации, соответствующей типу помещения (общая для всех типов КА), отсортированные по приоритету
			/// </summary>
			public IEnumerable<RecomendationItem> RoomTypeRecomendationItems =>
				RoomTypeRecomendation?.Items?
				.OrderBy(x => x.Priority)
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();

			/// <summary>
			/// Строки рекомендации, соответствующей типу КА (общая для всех типов помещений), отсортированные по приоритету
			/// </summary>
			public IEnumerable<RecomendationItem> PersonTypeRecomendationItems =>
				PersonTypeRecomendation?.Items?
				.OrderBy(x => x.Priority)
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();
		}
	}
}
