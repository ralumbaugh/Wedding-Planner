using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
    [Key]
    public int WeddingId {get; set;}
    [Required (ErrorMessage="Both Wedders are required")]
    public string WedderOne {get; set;}
    [Required (ErrorMessage="Both Wedders are required")]
    public string WedderTwo {get; set;}
    [Required (ErrorMessage="A wedding date is required")]
    public DateTime? Date {get; set;}
    [Required (ErrorMessage="It has to take place somewhere. Where?")]
    public string Address {get; set;}
    public int UserId {get; set;}
    public User Planner {get; set;}
    public List<WeddingAttendees> Guests {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;

    public bool CheckForUser(List<WeddingAttendees> Guests, int CurrentID)
    {
        bool Present = false;
        foreach(WeddingAttendees guest in Guests)
        {
            if(guest.UserId == CurrentID)
            {
                Present = true;
            }
        }
        return Present;
    }
    }
    
}