using System;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    [Route("Weddings")]
    public class WeddingsController : Controller
    {
        // Sets up context to be used to Query the DB
        private HomeContext dbContext;
        public WeddingsController(HomeContext context)
        {
            dbContext = context;
        }

        // <---------- Weddings GET routes ---------->
        [HttpGet("")]
        public IActionResult Index()
        {
            // Retrieves data from session to query as an event handler
            // checks to see if the session data is present to prevent 
            // penetration.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");
            string email = HttpContext.Session.GetString("Email");

            // If loggedIn not present, proceed to default View
            if (loggedIn == null)
            {
                return View("Index", "Users");
            }
            // Else checks to see if the user is in the DB
            else
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == email);
                if(userInDb == null)
                {
                    // If user is not in DB, kills session, returns Index
                    HttpContext.Session.Clear();
                    return View("Index", "Users");
                }
                else
                {
                    // Checks to see if the Session UserId == actual UserId
                    if(userInDb.UserId != (int)userId)
                    {
                        // If the UserId's do not match, kills session, returns Index
                        HttpContext.Session.Clear();
                        return View("Index", "Users");
                    }
                    // If all checks pass, redirects to the Wedding Dashboard
                    else
                    {
                        // Scrub for any weddings that are older than today's date
                        // Delete them from the DB
                        List<Wedding> ExpiredWeddings = dbContext.Weddings.Where(w => w.Date < DateTime.Now).ToList();
                        foreach(var wedding in ExpiredWeddings)
                        {
                            dbContext.Weddings.Remove(wedding);
                            dbContext.SaveChanges();
                        }
                        
                        // Query of all weddings to list for the index page
                        ViewBag.UserId = (int)userId;
                        ViewBag.LoggedIn = "LoggedIn";

                        List<Wedding> weddings = dbContext.Weddings
                            .Include(w => w.Attendees)
                            .OrderByDescending(w => w.Date)
                            .ToList();

                        return View(weddings);
                    }
                }
            }
        }

        // RESTful route for wedding registration form.
        [HttpGet("New")]
        public IActionResult New()
        {
            // Checks to see if loggedIn is present.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");

            // If loggedIn is not present, then proceed
            if(loggedIn == null)
            {
                return View("Index", "Users");
            }
            else
            {
                ViewBag.LoggedIn = "LoggedIn";
                ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
                return View("New");
            }
        }

        // RESTful route for showing individual wedding.
        [HttpGet("{weddingId}")]
        public IActionResult Show(int weddingId)
        {
            // Checks to see if loggedIn is present.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");

            // If loggedIn is not present, then proceed
            if(loggedIn == null)
            {
                return View("Index", "Users");
            }
            else
            {
                ViewBag.LoggedIn = "LoggedIn";

                // Query the DB for the routed wedding
                Wedding wedding = dbContext.Weddings
                .Include(w => w.Attendees)
                .ThenInclude(a => a.Guest)
                .FirstOrDefault(w => w.WeddingId == weddingId);

                return View(wedding);
            }
        }

        // <---------- weddings POST routes to the DB ---------->

        [HttpPost("Create")]
        public IActionResult Create(Wedding wedding)
        {
            // Checks to see if loggedIn is present.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");

            // If loggedIn is not present, then proceed
            if(loggedIn == null)
            {
                return View("Index", "Users");
            }
            else
            {
                if(ModelState.IsValid)
                {
                    ViewBag.LoggedIn = "LoggedIn";

                    // updates the DateTime values for the DB entry
                    wedding.CreatedAt = DateTime.Now;
                    wedding.UpdatedAt = DateTime.Now;
                    wedding.UserId = (int)userId;

                    // Queries the DB to add the Wedding object to the DB
                    dbContext.Weddings.Add(wedding);
                    dbContext.SaveChanges();
                    return RedirectToAction("Show", new{weddingId = wedding.WeddingId});
                }
                else
                {
                    ViewBag.LoggedIn = "LoggedIn";
                    return View("New");
                }
            }
        }
        [HttpPost("Destroy/{weddingId}")]
        public IActionResult Destroy(int weddingId)
        {
            // Retrieves data from session to query as an event handler
            // checks to see if the session data is present to prevent 
            // penetration.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");
            string email = HttpContext.Session.GetString("Email");

            // If loggedIn not present, proceed to default View
            if (loggedIn == null)
            {
                return View("Index", "Users");
            }
            // Else checks to see if the user is in the DB
            else
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == email);
                if(userInDb == null)
                {
                    // If user is not in DB, kills session, returns Index
                    HttpContext.Session.Clear();
                    return View("Index", "Users");
                }
                else
                {
                    // Checks to see if the Session UserId == actual UserId
                    if(userInDb.UserId != (int)userId)
                    {
                        // If the UserId's do not match, kills session, returns Index
                        HttpContext.Session.Clear();
                        return View("Index", "Users");
                    }
                    // If all checks pass, deletes the wedding
                    else
                    {
                        // Query of all weddings to delete the selected wedding
                        Wedding deleteWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId && w.UserId == userId);

                        // If query null, redirect to the Index
                        if(deleteWedding == null)
                        {
                            ViewBag.LoggedIn = "LoggedIn";
                            return RedirectToAction("Index");
                        }
                        // Else query to delete the wedding
                        else
                        {
                            ViewBag.LoggedIn = "LoggedIn";
                            dbContext.Weddings.Remove(deleteWedding);
                            dbContext.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
        }
    }
}