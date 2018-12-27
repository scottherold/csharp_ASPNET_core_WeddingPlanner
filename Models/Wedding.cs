using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    // Model used for New Wedding
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set;}

        // Wedder 1 validators
        [Display(Name="Wedder One")]
        [Required(ErrorMessage="Wedder one must be present!")]
        [MinLength(2,ErrorMessage="Wedder one name must be at least two characters!")]
        public string Wedder1 { get; set; }

        // Wedder 2 validators
        [Display(Name="Wedder Two")]
        [Required(ErrorMessage="Wedder two must be present!")]
        [MinLength(2,ErrorMessage="Wedder two name must be at least two characters!")]
        public string Wedder2 { get; set; }

        // Wedding date validators
        [Display(Name="Wedding Date")]
        [Required(ErrorMessage="Wedding date is required!")]
        [DataType(DataType.Date,ErrorMessage="Wedding Date must be a date!")]
        public DateTime Date { get; set; }

        // Address validators
        [Display(Name="Wedding Venue Address")]
        [Required(ErrorMessage="Wedding venue address is required!")]
        [MinLength(10,ErrorMessage="Wedding venue address must be at least 10 characters!")]
        public string Address { get; set; }

        // For database entry timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // PK for Many-to-one relationship with the User model
        public int UserId { get; set; }

        // Entity User reference
        public User Creator { get; set; }
        public List<Attendee> Attendees { get; set; }
    }

    // For the Many-to-Many relationship between wedding and guests
    // Adding class under wedding Model due to Wedding being the to which
    // Guests can attend.
    public class Attendee
    {
        [Key]
        public int AttendeeId {get; set; }
        public int UserId { get; set;}
        public int WeddingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User Guest { get; set; }
    }
}