using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.BaseEntity
{
    public interface IEntity
    {
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }

    public abstract class BaseEntity<TKey> : IEntity<TKey>, ISoftDelete
    {
        public TKey Id { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public BaseEntity()
        {
            CreatedAt = DateTime.Now;
        }
    }

    public abstract class BaseEntity : BaseEntity<int>
    {
    }

    public interface ISoftDelete
    {
        public bool IsDeleted { get; set; }

    }
}
