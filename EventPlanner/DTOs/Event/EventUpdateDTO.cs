using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.Event
{
    public class EventUpdateDTO
    {
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int CategoryId { get; set; }
        public int MaxParticipants { get; set; }
        public EventStatus Status { get; set; }


    }
}
