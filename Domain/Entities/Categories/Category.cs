using Domain.Entities.BaseEntity;
using Domain.Entities.Transactions;

namespace Domain.Entities.Categories
{
    public class Category : BaseEntity<long>
    {
        public string Name { get; set; }
        public long? parentId { get; set; }
        public Category Parent { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
