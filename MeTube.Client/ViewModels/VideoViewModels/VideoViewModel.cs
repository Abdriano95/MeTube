using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoViewModel
    {

        [ObservableProperty]
        public string _title;

        [ObservableProperty]
        public string _description;

        [ObservableProperty]
        public int _likes;

        [ObservableProperty]
        public ObservableCollection<string> _comments;

        public VideoViewModel()
        {
            _title = string.Empty;
            _description = string.Empty;
            _comments = new ObservableCollection<string>();
            FillPost();
        }

        public void FillPost()
        {
            List<string> comments = new List<string>
            {
                "Bra video!",
                "Härlig video",
                "Bra video!",
                "Härlig video",
                "Bra video!",
                "Härlig video"
            };

            foreach (var comment in comments)
            {
                Comments.Add(comment);
            }

            Title = "VSause, Michael here.";

            Description = "Ranting about unrelated things";

            Likes = 15;
        }
    }
}
