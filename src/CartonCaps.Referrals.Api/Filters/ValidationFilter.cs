using System.ComponentModel.DataAnnotations;

namespace CartonCaps.Referrals.Api.Filters;

/// <summary>
///     Simple endpoint filter that validates any complex request objects using DataAnnotations.
///     Returns 400 ValidationProblem if any validation errors are found.
/// </summary>
public sealed class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in context.Arguments)
        {
            if (arg is null) continue;

            var type = arg.GetType();

            // Skip primitive/builtin simple types
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                continue;

            var validationContext = new ValidationContext(arg);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(arg, validationContext, results, true);

            if (!isValid && results.Count > 0)
                foreach (var r in results)
                {
                    var member = r.MemberNames?.FirstOrDefault() ?? "";
                    if (string.IsNullOrWhiteSpace(member)) member = "_request";

                    if (!errors.TryGetValue(member, out var arr))
                    {
                        errors[member] = new[] { r.ErrorMessage ?? "Invalid value" };
                    }
                    else
                    {
                        var list = arr.ToList();
                        list.Add(r.ErrorMessage ?? "Invalid value");
                        errors[member] = list.ToArray();
                    }
                }
        }

        if (errors.Count > 0) return Results.ValidationProblem(errors, statusCode: StatusCodes.Status400BadRequest);

        return await next(context);
    }
}
