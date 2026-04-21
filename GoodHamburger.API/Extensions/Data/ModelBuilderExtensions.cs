using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Extensions.Data;

public static class ModelBuilderExtensions
{
    public static void UseSqliteCaseInsensitiveCollation(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var stringProperties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(string));

            foreach (var property in stringProperties)
            {
                property.SetCollation("NOCASE");
            }
        }
    }
}
