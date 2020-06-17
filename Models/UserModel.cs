using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class User
    {
    [Key]
    public int UserId {get; set;}
    [Required (ErrorMessage="First Name is Required")]
    public string FirstName {get;set;}
    [Required (ErrorMessage="Last Name is Required")]
    public string LastName {get;set;}
    [Required (ErrorMessage="Email is Required")]
    [EmailAddress (ErrorMessage="Please enter a valid email address")]
    public string Email {get; set;}
    [Required (ErrorMessage="Email is Required")]
    [DataType (DataType.Password)]
    [MinLength (8, ErrorMessage="Password must be at least 8 characters!")]
    public string Password {get; set;}
    public List<Wedding> WeddingsPlanned {get; set;}
    public List<WeddingAttendees> WeddingsAttending {get;set;}
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string Confirm {get; set;}
    }
    
}