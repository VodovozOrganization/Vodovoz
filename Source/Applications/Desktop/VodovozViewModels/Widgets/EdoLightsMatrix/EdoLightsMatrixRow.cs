using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.ViewModels.Widgets.EdoLightsMatrix
{
	public class LightsMatrixRow : PropertyChangedBase
	{
		public ReasonForLeaving ReasonForLeaving { get; set; }

		public string Title
		{
			get
			{
				if(ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
				{
					return "Для собственных\nнужд";
				}

				if(ReasonForLeaving == ReasonForLeaving.Resale)
				{
					return "Перепродажа";
				}

				return ReasonForLeaving.GetEnumTitle();
			}
		}

		public List<EdoLightsMatrixColumn> Columns { get; set; } = new List<EdoLightsMatrixColumn>();

		public void Colorize(EdoLightsMatrixColumn column, PossibleAccessState edoLightsColorizeType)
		{
			column.EdoLightsColorizeType = edoLightsColorizeType;
			OnPropertyChanged(nameof(GetColorizeType));
		}

		public PossibleAccessState GetColorizeType(EdoLightsMatrixPaymentType paymentKind)
		{
			var column = Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return PossibleAccessState.Forbidden;
			}

			return column.EdoLightsColorizeType;
		}
	}
}
