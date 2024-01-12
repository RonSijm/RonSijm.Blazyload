namespace RonSijm.Blazyload.MicrosoftServiceProvider;

public static class StringResources
{
    public static string Format(string error, string typeName)
    {
        return $"{error} - {typeName}";
    }

    public static string Format(string error, Type type)
    {
        return $"{error} - {type.FullName}";
    }

    public static string Format(string error, Type serviceType, Type type)
    {
        return $"{error} - {serviceType.FullName} - {type.FullName}";
    }

    public static string Format(string error, Type serviceType, string type)
    {
        return $"{error} - {serviceType.FullName} - {type}";
    }

    public static string Format(string error, Type serviceType, Type scopedService, string type)
    {
        return $"{error} - {serviceType.FullName} - {scopedService} - {type}";
    }


    public static string Format(string error, Type serviceType, Type type, string other1, string other2)
    {
        return $"{error} - {serviceType.FullName} - {type.FullName} - {other1} - {other2}";
    }
}