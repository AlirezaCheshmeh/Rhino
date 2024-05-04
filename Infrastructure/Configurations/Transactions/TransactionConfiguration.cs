using Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Transactions
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions", "Transaction");
            builder.HasOne(z => z.Category).WithMany(z => z.Transactions).HasForeignKey(z => z.CategoryId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(z => z.Bank).WithMany(z => z.Transactions).HasForeignKey(z => z.BankId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
