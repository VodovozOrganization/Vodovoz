using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

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

		public void SetAllow(EdoLightsMatrixColumn column, bool isAllowed)
		{
			column.IsAllowed = isAllowed;
			OnPropertyChanged(nameof(IsAllowed));
		}

		public bool IsAllowed(EdoLightsMatrixPaymentType paymentKind)
		{
			var column = Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return false;
			}

			return column.IsAllowed;
		}
	}
}
