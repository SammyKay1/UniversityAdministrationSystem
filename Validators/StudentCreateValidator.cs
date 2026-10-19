
using FluentValidation;

public class StudentCreateValidator
    : AbstractValidator<StudentCreateDto>
{
    public StudentCreateValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today.AddYears(-16))
            .WithMessage("Student must be at least 16 years old.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0);

        RuleFor(x => x.Address).SetValidator(new AddressDtoValidator());
            
    }
}