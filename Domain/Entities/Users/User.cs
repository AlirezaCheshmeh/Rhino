using Domain.Entities.BaseEntity;
using Domain.Entities.Roles;

namespace Domain.Entities.Users
{
    public class User : BaseEntity<long>
    {
        public User()
        {
            IsActive = false;
        }

        public string? FullName { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
        public long? TelegramId { get; set; }
        public int RoleId { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        //relations
        public virtual Role Role { get; set; }


        //set variables for update
        internal void SetMobile(string mobile) => Mobile = mobile;
        internal void SetEmail(string email) => Email = email;
        internal void SetFirstName(string firstName) => FirstName = firstName;
        internal void SetLastName(string lastName) => LastName = lastName;
        internal void SetRoleId(int roleId) => RoleId = roleId;

    }

    /// <summary>
    /// enums
    /// </summary>
    public enum UserStatus
    {
        InActive = 1,
        Active,
        PreRegister,
        Limited
    }
    public enum UserContactType
    {
        Phone = 1,
        PhoneNumber,
        Email,
        Discord,
        Telegram,
    }
    public enum UserImageType
    {
        Profile = 1,
        Licence,
        Docs,
    }

}