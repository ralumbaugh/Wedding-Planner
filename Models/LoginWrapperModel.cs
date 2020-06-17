using System;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class LoginWrapper
    {
        public User NewUser {get; set;}
        public LoginUser LoginUser {get; set;}
    }
}