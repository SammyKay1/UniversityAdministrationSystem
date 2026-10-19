using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DepartmentConfiguration
    : IEntityTypeConfiguration<Department>
{
    public void Configure(
        EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(d => d.Email)
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.HasIndex(d => d.Code)
            .IsUnique();

        //Global Soft-delete filter
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}