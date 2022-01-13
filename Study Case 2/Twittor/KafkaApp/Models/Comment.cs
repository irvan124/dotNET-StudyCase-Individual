using System;
using System.Collections.Generic;

#nullable disable

namespace KafkaApp.Models
{
    public partial class Comment
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public int TweetId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Tweet Tweet { get; set; }
        public virtual User User { get; set; }
    }
}
