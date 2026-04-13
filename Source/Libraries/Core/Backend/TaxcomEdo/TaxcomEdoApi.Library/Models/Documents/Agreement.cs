using System;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class Agreement : IAgreement
	{
		public string Name { get; set; }
		public DateTime Date { get; set; }
	}
}
