namespace EventPlanner.DTOs.Event
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Flattened from User
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // Flattened from Category
        public int MaxParticipants { get; set; }
        public string Status { get; set; }
        public int RSVPCount { get; set; }
        public int InviteCount { get; set; }
    }
}
