using VodovozBusiness.Domain.Common;

namespace VodovozBusiness.Domain.Client
{
	public class CounterpartyFileInformation : FileInformation
	{
		private int _counterpartyId;

		public int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
	}
}
