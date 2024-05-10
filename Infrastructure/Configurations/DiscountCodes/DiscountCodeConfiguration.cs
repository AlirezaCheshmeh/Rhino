using Domain.Entities.DiscountCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.DiscountCodes
{
    public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
    {
        public void Configure(EntityTypeBuilder<DiscountCode> builder)
        {
            builder.ToTable("DiscountCode", "Plan");
            builder.HasOne(z=>z.Plan).WithMany(x=>x.DiscountCodes).HasForeignKey(z=>z.PlanId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
