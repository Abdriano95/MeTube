using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.HistoryViewModels
{
    [ObservableObject]
    public partial class AdminHistoryViewModel
    {
        private readonly IAdminHistoryService _adminHistoryService;
        private readonly IUserService _userService;
        private readonly IVideoService _videoService;

        public AdminHistoryViewModel(
            IAdminHistoryService adminHistoryService,
            IUserService userService,
            IVideoService videoService)
        {
            _adminHistoryService = adminHistoryService;
            _userService = userService;
            _videoService = videoService;

            Histories = new ObservableCollection<HistoryAdmin>();
            EditingHistory = new HistoryAdmin
            {
                DateWatched = DateTime.Now
            };

            Users = new ObservableCollection<UserDetails>();
            Videos = new ObservableCollection<Video>();
        }

        // ------------------------------------------------------------------
        // Collections och properties
        // ------------------------------------------------------------------
        [ObservableProperty]
        private ObservableCollection<HistoryAdmin> _histories;

        [ObservableProperty]
        private ObservableCollection<UserDetails> _users;

        [ObservableProperty]
        private ObservableCollection<Video> _videos;

        // I stället för att binda till en hel user, binder vi enbart mot Id
        [ObservableProperty]
        private int _selectedUserId;

        // Samma för video
        [ObservableProperty]
        private int _selectedVideoId;

        [ObservableProperty]
        private HistoryAdmin _editingHistory;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _infoMessage;

        // Hjälpproperty: när vi behöver accessa det *faktiska* user-objektet
        public UserDetails? SelectedUser => Users.FirstOrDefault(u => u.Id == SelectedUserId);

        public Video? SelectedVideo => Videos.FirstOrDefault(v => v.Id == SelectedVideoId);

        // ------------------------------------------------------------------
        // Ladda all users och videos (vid sidstart)
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task LoadAllUsersAndVideosAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                Users.Clear();
                var allUsers = await _userService.GetAllUsersDetailsAsync();
                foreach (var u in allUsers)
                    Users.Add(u);

                Videos.Clear();
                var allVideos = await _videoService.GetAllVideosAsync();
                foreach (var v in allVideos)
                    Videos.Add(v);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading users/videos: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------------------------------------------------------------
        // Ladda en användares historik
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task LoadHistoriesAsync()
        {
            if (SelectedUserId == 0)
            {
                ErrorMessage = "Please select a user first!";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                Histories.Clear();

                var list = await _adminHistoryService.GetHistoryByUserAsync(SelectedUserId);
                foreach (var item in list)
                {
                    Histories.Add(item);
                }

                if (Histories.Count == 0)
                {
                    InfoMessage = $"User {SelectedUserId} has no history records yet.";
                }
                else
                {
                    InfoMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading histories: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------------------------------------------------------------
        // Skapa en ny post
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task CreateHistoryAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Sätt user & video i EditingHistory
                if (SelectedUser != null)
                {
                    EditingHistory.UserId = SelectedUser.Id;
                    EditingHistory.UserName = SelectedUser.Username;
                }
                if (SelectedVideo != null)
                {
                    EditingHistory.VideoId = SelectedVideo.Id;
                    EditingHistory.VideoTitle = SelectedVideo.Title;
                }

                var created = await _adminHistoryService.CreateHistoryAsync(EditingHistory);
                if (created == null)
                {
                    ErrorMessage = "Failed to create history. Check logs or server response.";
                    return;
                }

                Histories.Add(created);

                // Nollställ "EditingHistory"
                EditingHistory = new HistoryAdmin
                {
                    DateWatched = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------------------------------------------------------------
        // Uppdatera en befintlig post
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task UpdateHistoryAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (SelectedUser != null)
                {
                    EditingHistory.UserId = SelectedUser.Id;
                    EditingHistory.UserName = SelectedUser.Username;
                }
                if (SelectedVideo != null)
                {
                    EditingHistory.VideoId = SelectedVideo.Id;
                    EditingHistory.VideoTitle = SelectedVideo.Title;
                }

                bool success = await _adminHistoryService.UpdateHistoryAsync(EditingHistory);
                if (!success)
                {
                    ErrorMessage = "Failed to update history.";
                    return;
                }

                // Uppdatera i listan
                var index = Histories.ToList().FindIndex(h => h.Id == EditingHistory.Id);
                if (index >= 0)
                {
                    Histories[index] = new HistoryAdmin
                    {
                        Id = EditingHistory.Id,
                        UserId = EditingHistory.UserId,
                        UserName = EditingHistory.UserName,
                        VideoId = EditingHistory.VideoId,
                        VideoTitle = EditingHistory.VideoTitle,
                        DateWatched = EditingHistory.DateWatched
                    };
                }

                EditingHistory = new HistoryAdmin
                {
                    DateWatched = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------------------------------------------------------------
        // Ta bort
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task DeleteHistoryAsync(HistoryAdmin history)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                bool success = await _adminHistoryService.DeleteHistoryAsync(history.Id);
                if (!success)
                {
                    ErrorMessage = "Failed to delete history.";
                    return;
                }

                Histories.Remove(history);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------------------------------------------------------------
        // Börja "redigera" en vald post
        // ------------------------------------------------------------------
        public void EditHistory(HistoryAdmin history)
        {
            EditingHistory = new HistoryAdmin
            {
                Id = history.Id,
                UserId = history.UserId,
                UserName = history.UserName,
                VideoId = history.VideoId,
                VideoTitle = history.VideoTitle,
                DateWatched = history.DateWatched
            };

            // Synka dropdowns: Sätt SelectedUserId, SelectedVideoId
            SelectedUserId = history.UserId;
            SelectedVideoId = history.VideoId;
        }
    }
}
