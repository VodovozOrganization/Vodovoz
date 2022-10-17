using QS.Dialog;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client.EdoLightsMatrix
{
	public class EdoLightsMatrix
	{
		private readonly IInteractiveService _interactiveService;
		public GenericObservableList<LightsMatrixRow> LightsMatrixRows = new GenericObservableList<LightsMatrixRow>();

		public EdoLightsMatrix(IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService?? throw new ArgumentNullException(nameof(interactiveService));
		}
		public void SetAllow(ReasonForLeaving reasonForLeaving, EdoPaymentType paymentKind, bool isAllowed)
		{
			var row = LightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeaving);
			var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return;
			}

			row.SetAllow(column, isAllowed);
		}

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

		public void BindWithSource(Counterparty counterparty, ReasonForLeaving reasonForLeaving, EdoPaymentType edoPaymentType,
			PersonType personType, Light light)
		{
			counterparty.PropertyChanged += (sender, args) =>
			{
				foreach(var propertyName in light.PropertyNamesForLightWhenChanged)
				{
					if(args.PropertyName == propertyName && counterparty.PersonType == personType)
					{
						var isAllowed = light.LightCondition.Invoke();

						SetAllow(reasonForLeaving, edoPaymentType, isAllowed);
					}
				}

			};
		}

		public class Light
		{
			public Func<bool> LightCondition { get; set; }
			public string[] PropertyNamesForLightWhenChanged { get; set; }
		}

		public class Validation
		{
			public Func<bool> ValidationCondition { get; set; }
			public string ValidationMessage { get; set; }
		}
	}
}
