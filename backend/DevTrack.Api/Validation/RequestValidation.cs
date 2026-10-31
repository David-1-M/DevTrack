
using System.ComponentModel.DataAnnotations;

namespace DevTrackAPI.Validation;

public static class RequestValidation
{
    public static Dictionary<string, string[]> Validate<T>(
        T model) where T : class
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true
        );

        return results
            .SelectMany(result =>
                result.MemberNames
                    .DefaultIfEmpty("request")
                    .Select(name => new
                    {
                        Name = name,
                        Error = result.ErrorMessage
                            ?? "Invalid value"
                    }))
            .GroupBy(item => item.Name)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Error)
                    .ToArray()
            );
    }
}
