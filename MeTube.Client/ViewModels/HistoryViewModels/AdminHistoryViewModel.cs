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

        // 1) Constructor
        public AdminHistoryViewModel(IAdminHistoryService adminHistoryService)
        {
            _adminHistoryService = adminHistoryService;
            Histories = new ObservableCollection<HistoryAdmin>();
            EditingHistory = new HistoryAdmin();
        }

        // 2) Fields/Properties
        [ObservableProperty]
        private int _selectedUserId;

        [ObservableProperty]
        private ObservableCollection<HistoryAdmin> _histories;

        [ObservableProperty]
        private HistoryAdmin _editingHistory;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        // 3) Commands/Methods

        /// <summary>
        /// Load all history for given UserId.
        /// </summary>
        [RelayCommand]
        public async Task LoadHistoriesAsync()
        {
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
                    ErrorMessage = "No history found for this user or an error occurred.";
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

        /// <summary>
        /// Create a new history-post (POST).
        /// </summary>
        [RelayCommand]
        public async Task CreateHistoryAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Call AdminHistoryService
                var created = await _adminHistoryService.CreateHistoryAsync(EditingHistory);
                if (created == null)
                {
                    ErrorMessage = "Failed to create history. Check logs or server response.";
                    return;
                }

                // Add to list
                Histories.Add(created);

                // Reset input
                EditingHistory = new HistoryAdmin();
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

        /// <summary>
        /// Update existing history-post (PUT).
        /// </summary>
        [RelayCommand]
        public async Task UpdateHistoryAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                bool success = await _adminHistoryService.UpdateHistoryAsync(EditingHistory);
                if (!success)
                {
                    ErrorMessage = "Failed to update history. Possibly not found or server error.";
                    return;
                }

                // Update local list
                var index = Histories
                    .ToList()
                    .FindIndex(h => h.Id == EditingHistory.Id);

                if (index >= 0)
                {
                    // Replace with a new copy (to trigger UI refresh)
                    Histories[index] = new HistoryAdmin
                    {
                        Id = EditingHistory.Id,
                        UserId = EditingHistory.UserId,
                        VideoId = EditingHistory.VideoId,
                        UserName = EditingHistory.UserName,
                        VideoTitle = EditingHistory.VideoTitle,
                        DateWatched = EditingHistory.DateWatched
                    };
                }

                // clear/edit-läget
                EditingHistory = new HistoryAdmin();
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

        /// <summary>
        /// Remove history-post (DELETE).
        /// </summary>
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
                    ErrorMessage = "Failed to delete history. Possibly not found or server error.";
                    return;
                }

                // Remove from the local list
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

        /// <summary>
        /// Method for filling EditingHistory at "edit".
        /// </summary>
        public void EditHistory(HistoryAdmin history)
        {
            EditingHistory = new HistoryAdmin
            {
                Id = history.Id,
                UserId = history.UserId,
                VideoId = history.VideoId,
                UserName = history.UserName,
                VideoTitle = history.VideoTitle,
                DateWatched = history.DateWatched
            };
        }
    }
}
