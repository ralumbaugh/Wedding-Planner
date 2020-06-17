using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class WeddingAttendees
    {
        [Key]
        public int WeddingAttendeeId {get; set;}
        public User Guest {get; set;}
        public int UserId {get; set;}
        public Wedding Wedding {get; set;}
        public int WeddingId {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}