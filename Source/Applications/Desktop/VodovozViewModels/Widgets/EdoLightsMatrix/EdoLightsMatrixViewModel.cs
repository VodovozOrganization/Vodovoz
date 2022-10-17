using QS.Services;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Widgets.EdoLightsMatrix
{
	public class EdoLightsMatrixViewModel : EntityWidgetViewModelBase<Counterparty>
	{
		public EdoLightsMatrixViewModel(Counterparty entity,
			ICommonServices commonServices /*IEdoLightsMatrixController edoLightsMatrixController*/)
			: base(entity, commonServices)
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

		public void RefreshLightsMatrix()
		{
			UnLightAll();

			if(Entity.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, true);
			}


			if(Entity.ReasonForLeaving == ReasonForLeaving.Other)
			{
				if(Entity.PersonType == PersonType.legal)
				{
					Entity.IsNotSendDocumentsByEdo = true;
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

			if(Entity.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess ||
			   Entity.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
			{
				if(Entity.PersonType == PersonType.legal)
				{
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, true);
				}

				if(Entity.PersonType == PersonType.natural)
				{
					SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, true);
				}
			}
			else
			{
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, false);
				SetAllow(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, false);
			}

			if(Entity.ConsentForEdoStatus == ConsentForEdoStatus.Agree && Entity.PersonType == PersonType.legal)
			{
				SetAllow(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, true);
			}
		}
	}
}
