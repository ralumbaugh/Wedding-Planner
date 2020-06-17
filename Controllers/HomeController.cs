using System;
using System.Linq;
using System.Diagnostics;
using WeddingPlanner.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID!=null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            List<Wedding> AllWeddings = dbContext.Weddings
                .Include(w => w.Guests)
                .ThenInclude(g => g.Guest)
                .ToList();
            foreach(Wedding wedding in AllWeddings)
            {
                if(DateTime.Now.CompareTo(wedding.Date)>=0)
                {
                    dbContext.Weddings.Remove(wedding);
                    dbContext.SaveChanges();
                }
            }
            AllWeddings = dbContext.Weddings
                .Include(w=>w.Guests)
                .ThenInclude(g=>g.Guest)
                .ToList();
            WeddingWrapper Wrapper = new WeddingWrapper();
            Wrapper.CurrentUser = dbContext.Users.Include(g => g.WeddingsAttending).ThenInclude(w => w.Wedding).FirstOrDefault(user => user.UserId == (int)LoggedInUserID);
            Wrapper.AllWeddings = AllWeddings;
            return View(Wrapper);
        }
        [HttpGet("newwedding")]
        public IActionResult NewWedding()
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            WeddingWrapper Wrapper = new WeddingWrapper();
            int CurrentId = (int)LoggedInUserID;
            Wrapper.CurrentUser = dbContext.Users.FirstOrDefault(user => user.UserId==CurrentId);
            return View("NewWedding",Wrapper);
        }
        [HttpPost("CreateWedding")]
        public IActionResult CreateWedding(WeddingWrapper WrappedWedding)
        {
            Wedding ThisWedding = WrappedWedding.ThisWedding;
            ThisWedding.UserId = (int)HttpContext.Session.GetInt32("LoggedInUserID");
            if(DateTime.Now.CompareTo(ThisWedding.Date)>=0)
            {
                ModelState.AddModelError("ThisWedding.Date","The wedding should be planned in the future!");
            }
            if(ModelState.IsValid)
            {
                dbContext.Weddings.Add(ThisWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return NewWedding();
        }

        [HttpGet("individual/{WeddingId}")]
        public IActionResult IndividualWedding(int WeddingId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Wedding ThisWedding = dbContext.Weddings
                .Include(t => t.Guests)
                .ThenInclude(g => g.Guest)
                .FirstOrDefault(wedding => wedding.WeddingId == WeddingId);
            return View("IndividualWedding", ThisWedding);
        }

        [HttpGet("/rsvp/{WeddingId}")]
        public IActionResult RSVP(int WeddingId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            else if(dbContext.WeddingAttendees.Any(g => g.UserId==(int)LoggedInUserID && g.WeddingId == WeddingId) || dbContext.Weddings.FirstOrDefault(w=>w.WeddingId == WeddingId) == null)
            {
                return RedirectToAction("Dashboard");
            }
            WeddingAttendees NewLink = new WeddingAttendees();
            NewLink.UserId=(int)LoggedInUserID;
            NewLink.WeddingId=WeddingId;
            dbContext.WeddingAttendees.Add(NewLink);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [HttpGet("/cancel/{WeddingId}")]
        public IActionResult Cancel(int WeddingId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            WeddingAttendees RsvpToCancel = dbContext.WeddingAttendees.FirstOrDefault(w => w.WeddingId == WeddingId && w.UserId == (int)LoggedInUserID);
            if(RsvpToCancel != null)
            {
                dbContext.WeddingAttendees.Remove(RsvpToCancel);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("/delete/{WeddingId}")]
        public IActionResult Delete(int WeddingId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            Wedding WeddingToDelete = dbContext.Weddings.FirstOrDefault(wedding => wedding.WeddingId == WeddingId);
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            else if(WeddingToDelete != null && WeddingToDelete.UserId ==(int)LoggedInUserID)
            {
                dbContext.Weddings.Remove(WeddingToDelete);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }
        public IActionResult Login(LoginWrapper WrappedUser)
        {
            LoginUser user = WrappedUser.LoginUser;
            User UserInDb = dbContext.Users.FirstOrDefault(u=> u.Email == user.Email);
            if(UserInDb == null)
            {
                ModelState.AddModelError("LoginUser.Email", "The email/password combination is incorrect.");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(user, UserInDb.Password, user.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("LoginUser.Email", "The email/password combination is incorrect.");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("LoggedInUserID", UserInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        public IActionResult Register(LoginWrapper WrappedUser)
        {
            User user = WrappedUser.NewUser;
            if(dbContext.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("NewUser.Email", "Email already in use!");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("LoggedInUserID", user.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpGet("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
