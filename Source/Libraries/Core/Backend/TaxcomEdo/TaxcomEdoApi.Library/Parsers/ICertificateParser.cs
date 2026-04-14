namespace TaxcomEdoApi.Library.Parsers;

public interface ICertificateParser
{
	CertificateDetails Parse(string certificateSubject, string thumbprint);
}
