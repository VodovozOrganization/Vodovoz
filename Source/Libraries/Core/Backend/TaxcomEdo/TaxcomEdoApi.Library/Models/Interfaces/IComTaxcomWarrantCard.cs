using System;
using System.Collections.Generic;

namespace TaxcomEdoApi.Library.Models.Interfaces
{
	public interface IComTaxcomWarrantCard
	{
		IFileData Image { get; set; }

		IList<IFileData> Signs { get; set; }

		IComMeta Meta { get; set; }

		IList<string> DocSigns { get; set; }

		DateTime? DateStart { get; set; }

		DateTime? DateEnd { get; set; }

		bool IsMeta { get; set; }

		string ChildMetaWarrant { get; set; }

		string ChildFileWarrant { get; set; }

		bool IsFinal();
	}
}
