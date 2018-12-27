using System;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

// Controller for the Attendees Many-to-Many table

namespace WeddingPlanner.Controllers
{
    [Route("Attendees")]
    public class AttendeesController : Controller
    {
        // Sets up context to be used to Query the DB
        private HomeContext dbContext;
        public AttendeesController(HomeContext context)
        {
            dbContext = context;
        }

        // <---------- Database linking routes below ---------->
        
        // RESTful route for creating a new M2M Relationship
        [HttpPost("Create/{weddingId}")]
        public IActionResult Create(int weddingId)
        {
            // Retrieves data from session to query as an event handler
            // checks to see if the session data is present to prevent 
            // penetration.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");
            int? userId = HttpContext.Session.GetInt32("UserId");
            string email = HttpContext.Session.GetString("Email");

            Attendee attendee = new Attendee();

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
                    // If all checks pass, adds User as a guest to the wedding.
                    else
                    {
                        // Sets properties for the DB
                        attendee.UserId = (int)userId;
                        attendee.WeddingId = weddingId;
                        attendee.CreatedAt = DateTime.Now;
                        attendee.UpdatedAt = DateTime.Now;

                        dbContext.Attendees.Add(attendee);
                        dbContext.SaveChanges();
                        return RedirectToAction("Index", "Weddings");
                    }
                }
            }
        }

        // RESTful route for removing a new M2M Relationship
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
                    else
                    {
                        // Checks RSVP in DB to make sure it exists
                        Attendee rsvp = dbContext.Attendees.FirstOrDefault(a => a.UserId == (int)userId && a.WeddingId == weddingId);
                        
                        // if the RSVP is present, remove the RSVP
                        if(rsvp != null)
                        {
                            dbContext.Attendees.Remove(rsvp);
                            dbContext.SaveChanges();
                            return RedirectToAction("Index", "Weddings");
                        }

                        // Else redirects to the Index without accessing the DB
                        else
                        {
                            return RedirectToAction("Index", "Weddings");
                        }
                    }
                }
            }
        }
    }
}