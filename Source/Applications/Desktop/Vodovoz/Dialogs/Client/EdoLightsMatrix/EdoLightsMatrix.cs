using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client.EdoLightsMatrix
{
	public class EdoLightsMatrix
	{
		public GenericObservableList<LightsMatrixRow> LightsMatrixRows = new GenericObservableList<LightsMatrixRow>();

		private void CreateRow(ReasonForLeaving reasonForLeaving)
		{
			var lightsMatrixRow = new LightsMatrixRow
			{
				ReasonForLeaving = reasonForLeaving
			};

			foreach(EdoPaymentType paymentKind in (EdoPaymentType[])Enum.GetValues(typeof(EdoPaymentType)))
			{
				lightsMatrixRow.Columns.Add(new EdoLightsMatrixColumn
				{
					PaymentKind = paymentKind
				});
			}

			LightsMatrixRows.Add(lightsMatrixRow);
		}

		public void CreateRows(params ReasonForLeaving[] reasonsForLeaving)
		{
			foreach(var reason in reasonsForLeaving)
			{
				CreateRow(reason);
			}
		}

		public bool SetAllow(ReasonForLeaving reasonForLeaving, EdoPaymentType paymentKind, bool isAllowed)
		{
			var row = LightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeaving);
			var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return false;
			}

			row.SetAllow(column, isAllowed);

			return true;
		}
	}
}
