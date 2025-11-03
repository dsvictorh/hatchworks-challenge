using FluentValidation;

namespace CartonCaps.Referrals.Api.Filters;

/// <summary>
///     Endpoint filter that runs FluentValidation validators for complex request arguments.
///     Returns 400 ValidationProblem with aggregated errors when validation fails.
/// </summary>
public sealed class FluentValidationFilter : IEndpointFilter
{
    private readonly Dictionary<Type, IValidator> _validators;

    public FluentValidationFilter(IServiceProvider services)
    {
        // Build a cache of validators available in this assembly.
        _validators = new Dictionary<Type, IValidator>();
        var regs = AssemblyScanner.FindValidatorsInAssembly(typeof(Program).Assembly);
        foreach (var r in regs)
            if (r.InterfaceType.IsGenericType)
            {
                var targetType = r.InterfaceType.GetGenericArguments()[0];
                if (!_validators.ContainsKey(targetType))
                {
                    var instance = Activator.CreateInstance(r.ValidatorType) as IValidator;
                    if (instance != null) _validators[targetType] = instance;
                }
            }
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in context.Arguments)
        {
            if (arg is null) continue;
            var argType = arg.GetType();

            _validators.TryGetValue(argType, out var validator);
            if (validator == null) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
            if (!result.IsValid)
                foreach (var failure in result.Errors)
                {
                    var key = string.IsNullOrWhiteSpace(failure.PropertyName) ? "_request" : failure.PropertyName;
                    if (!errors.TryGetValue(key, out var arr))
                    {
                        errors[key] = new[] { failure.ErrorMessage };
                    }
                    else
                    {
                        var list = arr.ToList();
                        list.Add(failure.ErrorMessage);
                        errors[key] = list.ToArray();
                    }
                }
        }

        if (errors.Count > 0) return Results.ValidationProblem(errors, statusCode: StatusCodes.Status400BadRequest);

        return await next(context);
    }
}
