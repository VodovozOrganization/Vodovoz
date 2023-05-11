using System;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public abstract class ContactsFinderForExternalCounterparty
	{
		protected IContactFinderForExternalCounterparty NextHandler;
		
		public void SetNextHandler(IContactFinderForExternalCounterparty nextContactFinder)
		{
			NextHandler = nextContactFinder ?? throw new ArgumentNullException(nameof(nextContactFinder));
		}
		
		protected FoundContact NeedManualHandlingFoundContact()
		{
			return new FoundContact
			{
				FoundContactStatus = FoundContactStatus.NeedManualHandling
			};
		}
	}
}
