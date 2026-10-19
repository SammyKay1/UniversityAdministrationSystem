
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Email).IsUnique();
        
        builder.Property(p => p.ConcurrencyToken).IsConcurrencyToken();

        //Adding Global query filter for the soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}