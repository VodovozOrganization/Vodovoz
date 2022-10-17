using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Widgets.EdoLightsMatrix
{
	public class EdoLightsMatrixViewModel : WidgetViewModelBase
	{
		public EdoLightsMatrixViewModel()
		{
			CreateRows(ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Resale);
		}

		public GenericObservableList<LightsMatrixRow> ObservableLightsMatrixRows = new GenericObservableList<LightsMatrixRow>();

		private void SetAllow(ReasonForLeaving reasonForLeaving, EdoLightsMatrixPaymentType paymentKind, bool isAllowed)
		{
			var row = ObservableLightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeaving);
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

			foreach(EdoLightsMatrixPaymentType paymentKind in (EdoLightsMatrixPaymentType[])Enum.GetValues(
				        typeof(EdoLightsMatrixPaymentType)))
			{
				lightsMatrixRow.Columns.Add(new EdoLightsMatrixColumn
				{
					PaymentKind = paymentKind
				});
			}

			ObservableLightsMatrixRows.Add(lightsMatrixRow);
		}

		private void CreateRows(params ReasonForLeaving[] reasonsForLeaving)
		{
			foreach(var reason in reasonsForLeaving)
			{
				CreateRow(reason);
			}
		}

		private void BindWithSource(Counterparty counterparty, ReasonForLeaving reasonForLeaving, EdoLightsMatrixPaymentType edoPaymentType,
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
		private class Light
		{
			public Func<bool> LightCondition { get; set; }
			public string[] PropertyNamesForLightWhenChanged { get; set; }
		}

		private class Validation
		{
			public Func<bool> ValidationCondition { get; set; }
			public string ValidationMessage { get; set; }
		}

		private void UnLightAll()
		{
			foreach(var row in ObservableLightsMatrixRows)
			{
				foreach(var column in row.Columns)
				{
					SetAllow(row.ReasonForLeaving, column.PaymentKind, false);
				}
			}
		}

		public void RefreshLightsMatrix(Counterparty counterparty)
		{
			UnLightAll();

			if(counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Other)
			{
				if(counterparty.PersonType == PersonType.legal)
				{
					counterparty.IsNotSendDocumentsByEdo = true;
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, true);
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, true);
					SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
					SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
				}
				else
				{
					SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
				}
			}

			if(counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess ||
			   counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
			{
				if(counterparty.PersonType == PersonType.legal)
				{
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, true);
				}

				if(counterparty.PersonType == PersonType.natural)
				{
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, true);
				}
			}
			else
			{
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, false);
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, false);
			}

			if(counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree && counterparty.PersonType == PersonType.legal)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
			}

			if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess 
			    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
			   && counterparty.PersonType == PersonType.natural)
			{
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, true);
			}

			if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
			    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
			   && counterparty.PersonType == PersonType.legal)
			{
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, true);
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, true);
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
			}

			if(counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Blocked
				&& counterparty.PersonType == PersonType.natural)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
			}

			if(counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Blocked
			   && counterparty.PersonType == PersonType.legal)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
			}

			if(counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree
			   && counterparty.PersonType == PersonType.legal)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
			}
		}
	}
}
