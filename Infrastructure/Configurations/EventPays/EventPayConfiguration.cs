using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.EventPays;

namespace Infrastructure.Configurations.EventPays
{
    public class PeriodTransactionConfiguration : IEntityTypeConfiguration<EventPay>
    {
        public void Configure(EntityTypeBuilder<EventPay> builder)
        {
            builder.ToTable("EventPays", "Event");
        }
    }
}
