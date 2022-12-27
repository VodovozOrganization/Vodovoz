using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;

namespace Gamma.Binding
{
	public class ModelConfig<TNode, TParent> : IModelConfig {
		public Type Type { get; }
		public PropertyInfo ParentProperty { get; }
		public ModelConfig(Expression<Func<TNode, TParent>> parentPropertyExpr)
		{
			Type = typeof(TNode);
			ParentProperty = PropertyUtil.GetPropertyInfo(parentPropertyExpr);
		}

		public virtual IList GetChilds(object node = null) => new List<object>();
	}
	
	public class ModelConfig<TNode, TParent, TChild2> : ModelConfig<TNode, TParent>
	{
		public Expression<Func<TNode, IList<TParent>>> ChildsCollection1Property { get; }
		public Expression<Func<TNode, TChild2>> Child2Property { get; }
		public Expression<Func<TNode, IList<TChild2>>> ChildsCollection2Property { get; }

		public ModelConfig(
			Expression<Func<TNode, TParent>> parentPropertyExpr,
			Expression<Func<TNode, IList<TParent>>> childsCollection1PropertyExpr = null,
			Expression<Func<TNode, IList<TChild2>>> childsCollection2PropertyExpr = null) : base(parentPropertyExpr)
		{
			ChildsCollection1Property = childsCollection1PropertyExpr;
			ChildsCollection2Property = childsCollection2PropertyExpr;
		}
		
		public ModelConfig(
			Expression<Func<TNode, TParent>> parentPropertyExpr,
			Expression<Func<TNode, IList<TParent>>> childsCollection1PropertyExpr,
			Expression<Func<TNode, TChild2>> child2PropertyExpr) : base(parentPropertyExpr)
		{
			ChildsCollection1Property = childsCollection1PropertyExpr;
			Child2Property = child2PropertyExpr;
		}

		public override IList GetChilds(object node) {
			var list = new List<object>();

			if(ChildsCollection1Property != null) {
				var childs1 = ChildsCollection1Property.Compile().Invoke((TNode)node);
				foreach(var child in childs1) {
					list.Add(child);
				}
			}

			if(ChildsCollection2Property != null) {
				var childs2 = ChildsCollection2Property.Compile().Invoke((TNode)node);
				foreach(var child in childs2) {
					list.Add(child);
				}
			}

			if(Child2Property != null) {
				var child2 = Child2Property.Compile().Invoke((TNode)node);
				if(child2 != null) {
					list.Add(child2);
				}
			}

			return list;
		}
	}
}
