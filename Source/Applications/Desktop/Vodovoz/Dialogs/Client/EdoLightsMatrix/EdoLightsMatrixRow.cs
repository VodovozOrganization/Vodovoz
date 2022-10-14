using Gamma.Utilities;
using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client.EdoLightsMatrix
{
	public class LightsMatrixRow : PropertyChangedBase
	{
		public ReasonForLeaving ReasonForLeaving { get; set; }

		public string Title => ReasonForLeaving.GetEnumTitle();

		public List<EdoLightsMatrixColumn> Columns { get; set; } = new List<EdoLightsMatrixColumn>();

		public void SetAllow(EdoLightsMatrixColumn column, bool isAllowed)
		{
			column.IsAllowed = isAllowed;
			OnPropertyChanged(nameof(IsAllowed));
		}

		public bool IsAllowed(EdoPaymentType paymentKind)
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
