using System;
namespace Vodovoz.Services
{
	public interface ISmsNotifierParametersProvider
	{
		string GetNewClientSmsTextTemplate();
	}
}
