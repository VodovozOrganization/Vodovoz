using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Payments
{
	public class CombinedPaymentNode : IDomainObject
	{
		public bool IsTopLevel { get; set; }
		public PaymentType? PaymentType { get; set; }
		public string PaymentSubType { get; set; }
		public int Id { get; set; }
		public string Title { get; set; }
		public int? TopLevelId { get; set; }
		public IList<CombinedPaymentNode> Childs { get; set; }

		public static string[] GetTopLevelPaymentTypesNames(IEnumerable<CombinedPaymentNode> paymentNodes)
		{
			var topLevelPaymmentTypes =
				paymentNodes
				.Where(x => x.IsTopLevel)
				.Select(x => x.PaymentType?.ToString())
				.ToArray();

			return topLevelPaymmentTypes;
		}

		public static string[] GetTerminalPaymentTypeSubtypesNames(IEnumerable<CombinedPaymentNode> paymentNodes)
		{
			var terminalPaymentTypeSubtypes =
				paymentNodes
				.Where(x => x.PaymentSubType != null && x.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal)
				.Select(x => x.PaymentSubType)
				.ToArray();

			return terminalPaymentTypeSubtypes;
		}
	}
}
