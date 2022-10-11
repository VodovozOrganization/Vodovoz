using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ApiClientProvider
{
	public interface ICustomHttpClientFactory
	{
		HttpClient CreateClient();
	}
}
