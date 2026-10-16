namespace RaceDay.Api.DTOs
{
    public class ResultRequest
    {
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
        public bool Published { get; set; }
    }
}
