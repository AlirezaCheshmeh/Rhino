using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations.Role
{
    public class RoleConfiguration : IEntityTypeConfiguration<Domain.Entities.Roles.Role>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Roles.Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(x => x.Id);
            builder.HasMany(z => z.Users).WithOne(z => z.Role).HasForeignKey(z => z.RoleId);
            List<Domain.Entities.Roles.Role> seedData = new()
            {
                new Domain.Entities.Roles.Role{Id=1,RoleName="SuperAdmin"},
                new Domain.Entities.Roles.Role{Id=2,RoleName="Admin"},
                new Domain.Entities.Roles.Role{Id=3,RoleName="User"},
            };
            builder.HasData(seedData.ToArray());
        }
    }
}
