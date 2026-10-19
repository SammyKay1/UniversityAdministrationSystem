
using FluentValidation;

public class StudentEditValidator : AbstractValidator<StudentEditDto>
{
    public StudentEditValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Now.AddYears(-16))
            .WithMessage("Student must be at least 16 years old.");

        RuleFor(x => x.Address).SetValidator(new AddressDtoValidator());
                
        
    }
}