using System;

namespace TaxcomEdoApi.Library.Parsers;

public class CertificateDetails
{
	public string Thumbprint { get; set; }

	public string CommonName { get; set; }

	public string SurName { get; set; }

	public string GivenName { get; set; }

	public string CountryName { get; set; }

	public string StreetAddress { get; set; }

	public string LocalityName { get; set; }

	public string StateOrProvinceName { get; set; }

	public string OrganizationUnitName { get; set; }

	public string OrganizationName { get; set; }

	public string Title { get; set; }

	public string Ogrn { get; set; }

	public string OgrnIp { get; set; }

	public string Snils { get; set; }

	public string RnsFss { get; set; }

	public string KpFss { get; set; }

	public string Inn { get; set; }

	public string InnUl { get; set; }

	public string Issuer { get; set; }

	public string Email { get; set; }

	public DateTime? IssueDate { get; set; }
}
