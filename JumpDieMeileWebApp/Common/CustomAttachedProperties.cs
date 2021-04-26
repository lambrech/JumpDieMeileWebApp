namespace JumpDieMeileWebApp.Common
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public static class CustomAttachedProperties
    {
        private static readonly ConditionalWeakTable<object,
            Dictionary<string, object>> ObjectAttachedPropertiesMapping = new();

        public static void SetValue<T>(this T obj, string name, object value)
            where T : class
        {
            Dictionary<string, object> properties = ObjectAttachedPropertiesMapping.GetOrCreateValue(obj);

            if (properties.ContainsKey(name))
            {
                properties[name] = value;
            }
            else
            {
                properties.Add(name, value);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global - public usage is intended, but not used so far
        [return: MaybeNull]
        public static T GetValue<T>(this object obj, string name)
        {
            if (ObjectAttachedPropertiesMapping.TryGetValue(obj, out var properties) && properties.ContainsKey(name))
            {
                return (T)properties[name];
            }

            return default;
        }

        public static object? GetValue(this object obj, string name)
        {
            return obj.GetValue<object?>(name);
        }
    }
}