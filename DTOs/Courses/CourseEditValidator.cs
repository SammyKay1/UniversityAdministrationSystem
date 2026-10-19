using FluentValidation;




public class CourseEditValidator : AbstractValidator<CourseEditDto>
{
    public CourseEditValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.CourseCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.CreditHours)
            .InclusiveBetween(1, 6);

        RuleFor(x => x.Capacity)
            .GreaterThan(0);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0);
    }

    
}