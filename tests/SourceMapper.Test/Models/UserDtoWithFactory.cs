using System;
using SourceMapper.Test.Entities;

namespace SourceMapper.Test.Models
{
    [MapFrom(typeof(User))]
    public class UserDtoWithFactory : IEquatable<UserDtoWithFactory>
    {
        public string Username { get; set; }

        public string Name { get; set; }

        internal static UserDtoWithFactory Map(User user)
        {
            return new UserDtoWithFactory();
        }

        internal static UserDtoWithFactory Map(User user, bool showName, bool uppercaseName = false)
        {
            var name = uppercaseName ? user.Name.ToUpper() : user.Name;

            return new UserDtoWithFactory
            {
                Name = showName ? name : user.Username
            };
        }

        public bool Equals(UserDtoWithFactory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Username == other.Username && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserDtoWithFactory) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username, Name);
        }
    }
}
