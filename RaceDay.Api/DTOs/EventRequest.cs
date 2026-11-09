namespace RaceDay.Api.DTOs
{
    public class EventRequest
    {
        public int EventTypeId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public string? BannerImageUrl { get; set; }
    }
}
