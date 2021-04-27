namespace JumpDieMeileWebApp.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class AttributeHelper
    {
        public static IList<TAttribute> GetPropertyAttribute<T, TOut, TAttribute>(
            Expression<Func<T, TOut>> propertyExpression)
            where TAttribute : Attribute
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;
            return propertyInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>().ToList();
        }
    }
}