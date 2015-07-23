using System;
using QSBanks;

namespace Vodovoz.LoadFrom1c
{
	public class Account1c
	{
		public Account DomainAccount {get; set;}

		public string OwnerCode1c {get; set;}

		public Account1c ()
		{
			DomainAccount = new Account ();
		}
	}
}

