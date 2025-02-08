using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MeTube.Client.Models
{
    public class Comment : ObservableObject
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public string PosterUsername { get; set; }
    }
}
