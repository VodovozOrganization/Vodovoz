using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Specifications;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;
using Core.Infrastructure.Specifications;

namespace Vodovoz.ViewModels.Widgets.EdoLightsMatrix
{
	public class EdoLightsMatrixViewModel : WidgetViewModelBase
	{
		public EdoLightsMatrixViewModel()
		{
			CreateRows(ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Resale);
		}

		public GenericObservableList<LightsMatrixRow> ObservableLightsMatrixRows = new GenericObservableList<LightsMatrixRow>();

		private void Colorize(
			ReasonForLeaving reasonForLeaving,
			EdoLightsMatrixPaymentType paymentKind,
			PossibleAccessState edoLightsColorizeType)
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
					Colorize(row.ReasonForLeaving, column.PaymentKind, PossibleAccessState.Forbidden);
				}
			}
		}

		public void RefreshLightsMatrix(Counterparty counterparty)
		{
			UnLightAll();

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Unknown)
				.IsSatisfiedBy(counterparty))
			{
				return;
			}

			if(CounterpartySpecification
				.ReasonForLeavingIsIn(ReasonForLeaving.ForOwnNeeds, ReasonForLeaving.Other)
				.IsSatisfiedBy(counterparty))
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
			}

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Other)
				.And(CounterpartySpecification
					.IsLegalPerson())
				.IsSatisfiedBy(counterparty))
			{
				if(CounterpartySpecification
					.NonEmptyInn()
					.IsSatisfiedBy(counterparty))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
				}

				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
			}

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Resale)
				.IsSatisfiedBy(counterparty))
			{
				if(CounterpartySpecification
					.TruemarkRegisatrationStatusIsIn(RegistrationInChestnyZnakStatus.InProcess, RegistrationInChestnyZnakStatus.Registered)
					.And(CounterpartySpecification
						.NonEmptyInn())
					.And(CounterpartySpecification
						.IsLegalPerson())
					.IsSatisfiedBy(counterparty))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}

				if(CounterpartySpecification
					.TruemarkRegisatrationStatusIsIn(RegistrationInChestnyZnakStatus.InProcess, RegistrationInChestnyZnakStatus.Registered)
					.And(CounterpartySpecification
						.NonEmptyInn())
					.And(CounterpartySpecification.IsLegalPerson())
					.And(CounterpartySpecification.ConsentForEdoStatusIs(ConsentForEdoStatus.Agree))
					.IsSatisfiedBy(counterparty))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Cashless, PossibleAccessState.Allowed);
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}

				if(CounterpartySpecification
					.TruemarkRegisatrationStatusIsIn(RegistrationInChestnyZnakStatus.InProcess,
						RegistrationInChestnyZnakStatus.Registered)
					.And(CounterpartySpecification
						.NonEmptyInn())
					.And(CounterpartySpecification
						.IsNaturalPerson())
					.IsSatisfiedBy(counterparty))
				{
					Colorize(ReasonForLeaving.Resale, EdoLightsMatrixPaymentType.Receipt, PossibleAccessState.Allowed);
				}
			}

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.ForOwnNeeds)
				.And(CounterpartySpecification
					.IsLegalPerson())
				.IsSatisfiedBy(counterparty))
			{
				Colorize(ReasonForLeaving.ForOwnNeeds, EdoLightsMatrixPaymentType.Cashless,
					counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree
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

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Unknown)
				.IsSatisfiedBy(counterparty))
			{
				return false;
			}

			if(CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Other)
				.IsSatisfiedBy(counterparty))
			{
				if(CounterpartySpecification
					.IsLegalPerson()
					.IsSatisfiedBy(counterparty))
				{
					return true;
				}

				return paymentKind == EdoLightsMatrixPaymentType.Receipt;
			}

			return false;
		}
	}
}
