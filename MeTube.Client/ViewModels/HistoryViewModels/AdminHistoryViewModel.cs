using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace MeTube.Client.ViewModels.HistoryViewModels
{
    [ObservableObject]
    public partial class AdminHistoryViewModel
    {
        private readonly IAdminHistoryService _adminHistoryService;
        private readonly IUserService _userService;
        private readonly IVideoService _videoService;

        // NY PROPERTY: TempDateWatched (frikopplad från EditingHistory)
        [ObservableProperty]
        private DateTime _tempDateWatched = DateTime.Now;

        public AdminHistoryViewModel(
            IAdminHistoryService adminHistoryService,
            IUserService userService,
            IVideoService videoService)
        {
            _adminHistoryService = adminHistoryService;
            _userService = userService;
            _videoService = videoService;

            Histories = new ObservableCollection<HistoryAdmin>();

            EditingHistory = new HistoryAdmin();
            // TempDateWatched = DateTime.Now; // redan satt i fältdeklarationen

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

        [ObservableProperty]
        private int _selectedUserId;

        [ObservableProperty]
        private int _selectedVideoId;

        [ObservableProperty]
        private HistoryAdmin _editingHistory;

        [ObservableProperty]
        private DateTime _editingDate = DateTime.Now;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _infoMessage;

        public UserDetails? SelectedUser => Users.FirstOrDefault(u => u.Id == SelectedUserId);
        public Video? SelectedVideo => Videos.FirstOrDefault(v => v.Id == SelectedVideoId);

        // ------------------------------------------------------------------
        // Ladda alla users och videos (vid sidstart)
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task LoadAllUsersAndVideosAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                InfoMessage = string.Empty;

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
                ErrorMessage = $"Error loading users/videos: {ex.Message}";
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
            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;

            if (SelectedUserId == 0)
            {
                InfoMessage = "Please select a user first!";
                return;
            }

            try
            {
                IsLoading = true;
                Histories.Clear();

                var list = await _adminHistoryService.GetHistoryByUserAsync(SelectedUserId);
                foreach (var item in list)
                {
                    Histories.Add(item);
                }

                if (Histories.Count == 0)
                {
                    InfoMessage = $"No history found for user {SelectedUser?.Username}.";
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
        // Skapa en ny post (Create)
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task CreateHistoryAsync()
        {
            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;

            try
            {
                IsLoading = true;

                // 1) Sätt user & video
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

                // 2) Kopiera in vald tid
                EditingHistory.DateWatched = EditingDate;

                // 3) Validera
                EditingHistory.ValidateAll();
                if (EditingHistory.HasErrors)
                {
                    // Avbryt => fältfel visas i UI
                    return;
                }

                // 4) Spara via server
                var created = await _adminHistoryService.CreateHistoryAsync(EditingHistory);
                if (created == null)
                {
                    ErrorMessage = "Failed to create history. Check logs or server response.";
                    return;
                }

                // 5) Lägg till i listan
                Histories.Add(created);

                // 6) Nollställ
                EditingHistory = new HistoryAdmin();
                TempDateWatched = DateTime.Now;

                await LoadHistoriesAsync();
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
        // Uppdatera befintlig post (Update)
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task UpdateHistoryAsync()
        {
            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;

            try
            {
                IsLoading = true;

                // 1) Sätt user & video
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

                // 2) Kopiera in vald tid
                EditingHistory.DateWatched = EditingDate;

                // 3) Validera
                EditingHistory.ValidateAll();
                if (EditingHistory.HasErrors)
                {
                    return;
                }

                // 4) Anropa server
                bool success = await _adminHistoryService.UpdateHistoryAsync(EditingHistory);
                if (!success)
                {
                    ErrorMessage = "Failed to update history.";
                    return;
                }

                // 5) Uppdatera i listan
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

                // 6) Rensa
                EditingHistory = new HistoryAdmin();
                TempDateWatched = DateTime.Now;
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
            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;

            try
            {
                IsLoading = true;

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
        // Redigera en vald post
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

            // Sätt dropdowns
            SelectedUserId = history.UserId;
            SelectedVideoId = history.VideoId;

            // Sätt temp-datumet till det befintliga
            EditingDate = history.DateWatched;
        }
    }
}
