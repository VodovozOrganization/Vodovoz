using System;
using QS.ErrorReporting;
using Vodovoz.Services;

namespace Vodovoz.Infrastructure
{
	public class DefaultErrorDialogSettings : IErrorDialogSettings
	{
		public bool RequestEmail => false;

		public bool RequestDescription => true;
	}
}
