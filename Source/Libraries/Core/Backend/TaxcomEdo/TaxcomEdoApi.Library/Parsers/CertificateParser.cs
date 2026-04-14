using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TaxcomEdoApi.Library.Parsers;

public class CertificateParser : ICertificateParser
{
	private const string _keyGroup = "key";
	private const string _valueGroup = "value";
	private const string _matchPattern = $@"(?<{_keyGroup}>\w+)=(?<{_valueGroup}>[\w\W][^,]+)";
	private const string _surname = "SN";
	private const string _givenName = "G";
	private const string _commonName = "CN";
	private const string _country = "C";
	private const string _stateOrProvidence = "S";
	private const string _street = "STREET";
	private const string _locality = "L";
	private const string _inn = "ИНН";
	private const string _snils = "СНИЛС";
	private const string _ogrn = "ОГРН";
	private const string _organizationUnit = "OU";
	private const string _organization = "O";
	
	private readonly IDictionary<string, string> _subjectDetails = new Dictionary<string, string>();
	private string _processedThumbprint;

	public CertificateDetails Parse(string certificateSubject, string thumbprint)
	{
		if(string.IsNullOrWhiteSpace(_processedThumbprint)
			|| thumbprint != _processedThumbprint)
		{
			ParseSubject(certificateSubject);
			_processedThumbprint = thumbprint;
		}

		return FillCertificateDetails();
	}

	private CertificateDetails FillCertificateDetails()
	{
		var certDetails = new CertificateDetails
		{
			Thumbprint = _processedThumbprint
		};

		foreach(var keyPairValue in _subjectDetails)
		{
			switch(keyPairValue.Key)
			{
				case _surname:
					certDetails.SurName = keyPairValue.Value;
					break;
				case _givenName:
					certDetails.GivenName = keyPairValue.Value;
					break;
				case _commonName:
					certDetails.CommonName = keyPairValue.Value;
					break;
				case _inn:
					certDetails.Inn = keyPairValue.Value;
					break;
				case _snils:
					certDetails.Snils = keyPairValue.Value;
					break;
				case _ogrn:
					certDetails.Ogrn = keyPairValue.Value;
					break;
				case _organizationUnit:
					certDetails.OrganizationUnitName = keyPairValue.Value;
					break;
				case _organization:
					certDetails.OrganizationName = keyPairValue.Value;
					break;
				case _country:
					certDetails.CountryName = keyPairValue.Value;
					break;
				case _locality:
					certDetails.LocalityName = keyPairValue.Value;
					break;
				case _stateOrProvidence:
					certDetails.StateOrProvinceName = keyPairValue.Value;
					break;
				case _street:
					certDetails.StreetAddress = keyPairValue.Value;
					break;
			}
		}
		
		return certDetails;
	}

	private void ParseSubject(string subject)
	{
		var regex = new Regex(_matchPattern);
		var matches = regex.Matches(subject);

		if(matches is null || !matches.Any())
		{
			throw new InvalidOperationException("Не удалось получить данные подписанта из сертификата!");
		}

		foreach(Match match in matches)
		{
			_subjectDetails.Add(
				match.Groups[_keyGroup].Value,
				match.Groups[_valueGroup].Value);
		}
	}
}
