
using FluentValidation;

public abstract class BaseService
{
    protected async Task<OperationResult?> ValidateAsync<T>(IValidator<T> validator, T dto)
    {
        var result = await validator.ValidateAsync(dto);

        if (result.IsValid)
            return null;

        return OperationResult.Failure(
            string.Join(Environment.NewLine,
                result.Errors.Select(e => e.ErrorMessage)));
    }
}