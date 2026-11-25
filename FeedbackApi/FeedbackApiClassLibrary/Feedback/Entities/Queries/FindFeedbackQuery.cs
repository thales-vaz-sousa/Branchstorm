using System.ComponentModel.DataAnnotations;

namespace FeedbackApiClassLibrary.Feedback.Entities.Queries
{
    public class FindFeedbackQuery
    {
        [Required(ErrorMessage = "LinkCode is required")]
        public string LinkCode { get; init; }
    }
}
