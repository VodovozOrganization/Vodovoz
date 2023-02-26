using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrueMarkApi.Dto;
using TrueMarkApi.Services.Authorization;

namespace TrueMarkAuthorization
{
    internal class Program
    {
		public static async Task Main(string[] args)
		{
			var config = new ConfigurationBuilder()
			.SetBasePath(Environment.CurrentDirectory)
			.AddJsonFile("appsettings.Development.json")
			.Build();

			var apiSection = config.GetSection("Api");

			var organizationsCertificateSection = apiSection.GetSection("OrganizationCertificates");
			var _organizationCertificate = organizationsCertificateSection.Get<OrganizationCertificate[]>().ToArray().FirstOrDefault();

			var _authorizationService = new AuthorizationService(config);
			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint);

			Console.WriteLine(token);

			Console.ReadLine();
		}
	}
}
