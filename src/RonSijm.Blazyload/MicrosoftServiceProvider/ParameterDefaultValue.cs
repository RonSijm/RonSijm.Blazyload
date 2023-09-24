using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RonSijm.Blazyload.MicrosoftServiceProvider
{
    internal static class ParameterDefaultValue
    {
        public static bool TryGetDefaultValue(ParameterInfo parameter, out object defaultValue)
        {
            var hasDefaultValue = CheckHasDefaultValue(parameter, out var tryToGetDefaultValue);
            defaultValue = null;

            if (hasDefaultValue)
            {
                if (tryToGetDefaultValue)
                {
                    defaultValue = parameter.DefaultValue;
                }

                var isNullableParameterType = parameter.ParameterType.IsGenericType &&
                                              parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);

                // Workaround for https://github.com/dotnet/runtime/issues/18599
                if (defaultValue == null && parameter.ParameterType.IsValueType
                    && !isNullableParameterType) // Nullable types should be left null
                {
                    defaultValue = CreateValueType(parameter.ParameterType);
                }

                [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
                    Justification = "CreateValueType is only called on a ValueType. You can always create an instance of a ValueType.")]
                static object CreateValueType(Type t) =>
#if NETFRAMEWORK || NETSTANDARD2_0
                    FormatterServices.GetUninitializedObject(t);
#else
                    RuntimeHelpers.GetUninitializedObject(t);
#endif

                // Handle nullable enums
                if (defaultValue != null && isNullableParameterType)
                {
                    var underlyingType = Nullable.GetUnderlyingType(parameter.ParameterType);
                    if (underlyingType != null && underlyingType.IsEnum)
                    {
                        defaultValue = Enum.ToObject(underlyingType, defaultValue);
                    }
                }
            }

            return hasDefaultValue;
        }

        public static bool CheckHasDefaultValue(ParameterInfo parameter, out bool tryToGetDefaultValue)
        {
            tryToGetDefaultValue = true;
            return parameter.HasDefaultValue;
        }
    }
}