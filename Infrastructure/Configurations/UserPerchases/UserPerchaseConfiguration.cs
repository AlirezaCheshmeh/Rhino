using Domain.Entities.UserPurchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations.UserPerchases
{
    public class UserPerchaseConfiguration : IEntityTypeConfiguration<UserPurchase>
    {
        public void Configure(EntityTypeBuilder<UserPurchase> builder)
        {
            builder.ToTable("UserPerchases", "Plan");
            builder.HasOne(z => z.DiscountCode).WithMany(z => z.UserPurchases).HasForeignKey(z => z.DisCountCodeId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(z => z.Plan).WithMany(z => z.UserPurchases).HasForeignKey(z => z.PlanId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
