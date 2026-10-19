
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.CourseCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.CourseCode).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Capacity).IsRequired();
        builder.Property(c => c.CreditHours).IsRequired();
        builder.Property(c => c.IsArchived).HasDefaultValue(false);

        
    }
}