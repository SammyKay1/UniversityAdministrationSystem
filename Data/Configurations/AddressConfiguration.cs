
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AddressLine1)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(a => a.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(a => a.State)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(a => a.Country)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasOne(a => a.Person)
               .WithOne(p => p.Address)
               .HasForeignKey<Address>(a => a.PersonId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}