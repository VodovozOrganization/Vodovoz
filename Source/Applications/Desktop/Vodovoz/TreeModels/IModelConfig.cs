using System;
using System.Collections;
using System.Reflection;

namespace Gamma.Binding
{
	public interface IModelConfig {
		Type Type { get; }
		PropertyInfo ParentProperty { get; }
		IList GetChilds(object node);
	}
}
