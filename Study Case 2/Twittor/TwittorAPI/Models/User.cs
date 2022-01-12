using System;
using System.Collections.Generic;

#nullable disable

namespace TwittorAPI.Models
{
    public partial class User
    {
        public User()
        {
            Comments = new HashSet<Comment>();
            Tweets = new HashSet<Tweet>();
            UserRoles = new HashSet<UserRole>();
        }

        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Tweet> Tweets { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
