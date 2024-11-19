using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
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

		private void Colorize(ReasonForLeaving reasonForLeaving, EdoLightsMatrixPaymentType paymentKind, EdoLightsColorizeType edoLightsColorizeType)
		{
			var row = ObservableLightsMatrixRows.FirstOrDefault(c => c.ReasonForLeaving == reasonForLeaving);
			var column = row?.Columns?.FirstOrDefault(r => r.PaymentKind == paymentKind);

			if(column == null)
			{
				return;
			}

			row.Colorize(column, edoLightsColorizeType);
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
					Colorize(row.ReasonForLeaving, column.PaymentKind, EdoLightsColorizeType.Forbidden);
				}
			}
		}

		public void RefreshLightsMatrix(Counterparty counterparty)
		{
			UnLightAll();

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Unknown)
			{
				return;
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds || counterparty.ReasonForLeaving == ReasonForLeaving.Other)
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Other
			   && counterparty.PersonType == PersonType.legal)
			{
				if(!string.IsNullOrWhiteSpace(counterparty.INN))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, EdoLightsColorizeType.Allowed);
				}

				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, EdoLightsColorizeType.Allowed);
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.legal)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
				}

				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.legal
				   && counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, EdoLightsColorizeType.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
				}

				if((counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				    || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
				   && !string.IsNullOrWhiteSpace(counterparty.INN)
				   && counterparty.PersonType == PersonType.natural)
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, EdoLightsColorizeType.Allowed);
				}
			}

			if(counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds && counterparty.PersonType == PersonType.legal)
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless,
					counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						? EdoLightsColorizeType.Allowed
						: EdoLightsColorizeType.Unknown);
			}
		}

		public bool HasUnknown()
		{
			foreach(var row in ObservableLightsMatrixRows)
			{
				foreach(var column in row.Columns)
				{
					if(column.EdoLightsColorizeType == EdoLightsColorizeType.Unknown)
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
				return column.EdoLightsColorizeType == EdoLightsColorizeType.Allowed
					|| column.EdoLightsColorizeType == EdoLightsColorizeType.Unknown;
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
