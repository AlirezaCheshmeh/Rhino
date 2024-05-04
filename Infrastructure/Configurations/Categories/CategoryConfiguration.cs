using Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Categories
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories", "Category");
            builder.HasMany(z => z.Transactions).WithOne(z => z.Category).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
