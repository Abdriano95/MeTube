using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MeTube.Client.Models
{
    public partial class HistoryAdmin : ObservableValidator
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "User is required.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Video is required.")]
        public int VideoId { get; set; }

        [Required(ErrorMessage = "DateWatched is required.")]
        public DateTime DateWatched { get; set; } = DateTime.Now;

        public string UserName { get; set; } = string.Empty;
        public string VideoTitle { get; set; } = string.Empty;

        public User? User { get; set; }
        public Video? Video { get; set; }

        /// <summary>
        /// Manuell method to validate all properties.
        /// This is because ValidateAllProperties() is protected in ObservableValidator,
        /// we call ValidateProperty(...) ourselves.
        /// </summary>
        public void ValidateAll()
        {
            ValidateProperty(UserId, nameof(UserId));
            ValidateProperty(VideoId, nameof(VideoId));
            ValidateProperty(DateWatched, nameof(DateWatched));
        }
    }
}
