using Domain.Entities.Plans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Plans
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.ToTable("Plans", "Plan");
            builder.HasMany(z => z.UserPurchases).WithOne(x => x.Plan).HasForeignKey(z => z.PlanId).OnDelete(DeleteBehavior.NoAction);
            builder.HasMany(z => z.DiscountCodes).WithOne(x => x.Plan).HasForeignKey(z => z.PlanId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
