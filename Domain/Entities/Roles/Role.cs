

namespace Domain.Entities.Roles
{
    public class Role : BaseEntity.BaseEntity
    {
        public string RoleName { get; set; }
        public virtual ICollection<Users.User> Users { get; set; }
    }
}
