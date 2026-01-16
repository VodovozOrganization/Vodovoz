using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Extensions;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.ViewModels.Widgets.EdoLightsMatrix
{
	public class EdoLightsMatrixViewModel : WidgetViewModelBase
	{
		public EdoLightsMatrixViewModel()
		{
			CreateRows(ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Resale);
		}

		public GenericObservableList<LightsMatrixRow> ObservableLightsMatrixRows = new GenericObservableList<LightsMatrixRow>();

		private void Colorize(ReasonForLeaving reasonForLeaving, EdoLightsMatrixPaymentType paymentKind, PossibleAccessState edoLightsColorizeType)
		{
			var row = ObservableLightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeaving);
			var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return;
			}

			row.Colorize(column, edoLightsColorizeType);
		}
		
		private void Colorize(Dictionary<ReasonForLeaving, Dictionary<CounterpartyOrderPaymentType, PossibleAccessState>> matrix)
		{
			foreach(var reasonForLeavingKeyPairValue in matrix)
			{
				foreach(var counterpartyOrderPaymentTypeKeyPairValue in reasonForLeavingKeyPairValue.Value)
				{
					var paymentKind = counterpartyOrderPaymentTypeKeyPairValue.Key.ToEdoLightsMatrixPaymentType();
					var row = ObservableLightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeavingKeyPairValue.Key);
					var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);
					
					if(column == null)
					{
						return;
					}

					row.Colorize(column, counterpartyOrderPaymentTypeKeyPairValue.Value);
				}
			}
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

		private void UnLightAll()
		{
			foreach(var row in ObservableLightsMatrixRows)
			{
				foreach(var column in row.Columns)
				{
					Colorize(row.ReasonForLeaving, column.PaymentKind, PossibleAccessState.Forbidden);
				}
			}
		}

		//TODO 5606 восстановить после всех уточнений, при необходимости. Чтобы можно было хранить все условия в модели а не вью модели
		/*public void RefreshLightsMatrix(CounterpartyEdoAccount edoAccount)
		{
			UnLightAll();

			if(edoAccount is null)
			{
				return;
			}

			var counterparty = edoAccount.Counterparty;
			var matrix = counterparty.GetCanCounterpartyOrderMatrix(edoAccount);

			Colorize(matrix);
		}*/
		
		public void RefreshLightsMatrix(CounterpartyEdoAccount edoAccount)
		{
			UnLightAll();

			if(edoAccount is null)
			{
				return;
			}

			var counterparty = edoAccount.Counterparty;

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Unknown)
			{
				return;
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds || counterparty.ReasonForLeaving == ReasonForLeaving.Other)
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Other
			   && counterparty.PersonType == PersonType.legal)
			{
				if(!string.IsNullOrWhiteSpace(counterparty.INN))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
				}

				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.legal)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}

				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.legal
				   && edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}

				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.natural)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds && counterparty.PersonType == PersonType.legal)
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless,
					edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						? PossibleAccessState.Allowed
						: PossibleAccessState.Unknown);
			}
		}

		public bool HasUnknown()
		{
			foreach(var row in ObservableLightsMatrixRows)
			{
				foreach(var column in row.Columns)
				{
					if(column.EdoLightsColorizeType == PossibleAccessState.Unknown)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool IsPaymentAllowed(Counterparty counterparty, EdoLightsMatrixPaymentType paymentKind)
		{
			var row = ObservableLightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == counterparty.ReasonForLeaving);
			var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column != null)
			{
				return column.EdoLightsColorizeType == PossibleAccessState.Allowed
					|| column.EdoLightsColorizeType == PossibleAccessState.Unknown;
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Unknown)
			{
				return false;
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Other)
			{
				if(counterparty.PersonType == PersonType.legal)
				{
					return true;
				}

				return paymentKind == EdoLightsMatrixPaymentType.Receipt;
			}

			return false;
		}
	}
}
