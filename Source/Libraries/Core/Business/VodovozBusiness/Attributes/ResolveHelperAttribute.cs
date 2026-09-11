using System;
using System.Collections.Generic;

namespace VodovozBusiness.Attributes
{
	public class ResolveHelperAttribute : Attribute
	{
		public ResolveHelperAttribute(params Type[] types)
		{
			Types = types;
		}
		
		public IEnumerable<Type> Types { get; }
	}
}
