using System;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

// Controller for the User model

namespace WeddingPlanner.Controllers
{

    public class UsersController : Controller
    {
        // <---------- Context Setup ---------->

        // Sets up context to be used to Query the DB
        private HomeContext dbContext;
        public UsersController(HomeContext context)
        {
            dbContext = context;
        }

        // <---------- Users GET routes ---------->
        
        // Catch-all Http route! UsersController is the 'home' controller
        // due to being the login and registration app.
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
                return View();
            }
            // Else checks to see if the user is in the DB
            else
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == email);
                if(userInDb == null)
                {
                    // If user is not in DB, kills session, returns Index
                    HttpContext.Session.Clear();
                    return View();
                }
                else
                {
                    // Checks to see if the Session UserId == actual UserId
                    if(userInDb.UserId != (int)userId)
                    {
                        // If the UserId's do not match, kills session, returns Index
                        HttpContext.Session.Clear();
                        return View();
                    }
                    // If all checks pass, redirects to the Wedding Dashboard
                    else
                    {
                        ViewBag.LoggedIn = "LoggedIn";
                        return RedirectToAction("Index","Weddings");
                    }
                }
            }
        }

        // RESTful route for registration form.
        [HttpGet("New")]
        public IActionResult New()
        {
            // Checks to see if loggedIn is present.
            string loggedIn = HttpContext.Session.GetString("LoggedIn");

            // If loggedIn is not present, then proceed
            if(loggedIn == null)
            {
                return View();
            }
            // Else kill Session and proceed
            else
            {
                ViewBag.LoggedIn = null;
                HttpContext.Session.Clear();
                return View();
            }
        }

        // RESTful route for Logging Out of the server.
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            // Kills Session and returns to the Index.
                HttpContext.Session.Clear();
                return RedirectToAction("Index");
        }

        // <---------- Users POST routes to the DB ---------->

        // RESTful route for processing a request to a new user.
        [HttpPost("Create")]
        public IActionResult Create(User user)
        {
            // Validates form against model for field sanitization
            if(ModelState.IsValid)
            {
                // Checks to see if there is a duplicate email address
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually adds a ModelState error to the Email field
                    ModelState.AddModelError("Email", "Email is already in use!");
                    return View("New");
                }
                // Else creates the user
                else
                {
                    // Sets DateTime variables for the new user creation
                    user.CreatedAt = DateTime.Now;
                    user.UpdatedAt = DateTime.Now;

                    // Hashes the password
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);

                    // Adds user to the DB
                    dbContext.Add(user);
                    dbContext.SaveChanges();

                    // Creates 'Logged In' status, with security validation.
                    // Each route can now check to see if the User is logged in using
                    // session data to validate a query to the DB. If the email does 
                    // not match the email for the user id, then session will be cleared.
                    HttpContext.Session.SetString("LoggedIn", "Yes");
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Email", user.Email);

                    return RedirectToAction("Index","Weddings");
                }
            }
            // Else display field errors
            else
            {
                return View("New");
            }
        }

        // Route for logging in a new user.
        [HttpPost("Login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            // Validates form against model for field sanitization
            if(ModelState.IsValid)
            {
                // If there are no form errors, query the DB for the User's
                // Email in the DB to see if it exists
                var loginUser = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);

                // If user doesn't exist, throw a form field error
                // and return the Login view
                if(loginUser == null)
                {
                    ModelState.AddModelError("Password", "Invalid Email/Password!");
                    return View("Index");
                }
                // Else attempt to verify password
                else
                {
                    // Initializes password Hasher for login
                    var hasher = new PasswordHasher<LoginUser>();

                    // Verify provided password against has stored in db
                    var result = hasher.VerifyHashedPassword(userSubmission, loginUser.Password, userSubmission.Password);

                    // Result compared to 0 for failure
                    if(result == 0)
                    {
                        ModelState.AddModelError("Password", "Invalid Email/Password!");
                        return View("Index");
                    }
                    // Else creates 'Logged In' status, with security validation.
                    // Each route can now check to see if the User is logged in using
                    // session data to validate a query to the DB. If the email does 
                    // not match the email for the user id, then session will be cleared.
                    else
                    {
                        HttpContext.Session.SetString("LoggedIn", "Yes");
                        HttpContext.Session.SetInt32("UserId", loginUser.UserId);
                        HttpContext.Session.SetString("Email", loginUser.Email);

                        // Redirects to the Weddings Index, now "Logged In"
                        return RedirectToAction("Index","Weddings");
                    }
                }
            }
            // Else display field errors
            else
            {
                return View("Index");
            }
        }
    }
}