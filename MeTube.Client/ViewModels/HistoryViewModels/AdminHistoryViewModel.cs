using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;

using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for managing user watch history in admin interface. Handles CRUD operations
/// for history entries and maintains collections of users and videos.
/// </summary>
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
    // Collections and properties
    // ------------------------------------------------------------------

    /// <summary>
    /// Collection of watch history entries for the selected user
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<HistoryAdmin> _histories;

    /// <summary>
    /// Collection of all users in the system
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UserDetails> _users;

    /// <summary>
    /// Collection of all videos in the system
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Video> _videos;

    /// <summary>
    /// Currently selected user ID from dropdown
    /// </summary>
    [ObservableProperty]
    private int _selectedUserId;

    /// <summary>
    /// Currently selected video ID from dropdown
    /// </summary>
    [ObservableProperty]
    private int _selectedVideoId;

    // ObservableProperty doesn't work properly with DateTime, so we do this manually
    private HistoryAdmin _editingHistory;
    /// <summary>
    /// Currently edited history entry
    /// </summary>
    public HistoryAdmin EditingHistory
    {
        get => _editingHistory;
        set
        {
            if (_editingHistory != value)
            {
                _editingHistory = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if an operation is in progress
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// For server/network errors
    /// </summary>
    [ObservableProperty]
    private string _errorMessage;

    /// <summary>
    /// For general info messages, e.g., "No history found"
    /// </summary>
    [ObservableProperty]
    private string _infoMessage;

    // Helper props: Gets the complete user/video object
    /// <summary>
    /// Gets the complete UserDetails object for the selected user ID
    /// </summary>
    public UserDetails? SelectedUser => Users.FirstOrDefault(u => u.Id == SelectedUserId);

    /// <summary>
    /// Gets the complete Video object for the selected video ID
    /// </summary>
    public Video? SelectedVideo => Videos.FirstOrDefault(v => v.Id == SelectedVideoId);

    // ------------------------------------------------------------------
    // Load all users and videos
    // ------------------------------------------------------------------
    /// <summary>
    /// Loads all users and videos from the server and populates the corresponding collections.
    /// Clears existing data before loading new data.
    /// </summary>
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
    // Load user history
    // ------------------------------------------------------------------
    /// <summary>
    /// Loads watch history for the selected user. Requires a user to be selected (SelectedUserId != 0).
    /// Updates the Histories collection with the retrieved data.
    /// </summary>
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
    // Create new entry
    // ------------------------------------------------------------------
    /// <summary>
    /// Creates a new history entry using the data from EditingHistory.
    /// Validates the entry before sending to server and updates the Histories collection on success.
    /// </summary>
    [RelayCommand]
    public async Task CreateHistoryAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            InfoMessage = string.Empty;

            // Validation: User and Video must be selected
            if (SelectedUserId == 0 || SelectedVideoId == 0)
            {
                ErrorMessage = "Please select both a user and a video.";
                Console.WriteLine("[DEBUG] Validation failed: Missing User or Video.");
                return;
            }

            EditingHistory.UserId = SelectedUserId;
            EditingHistory.VideoId = SelectedVideoId;

            Console.WriteLine($"[DEBUG] Creating history: UserId={EditingHistory.UserId}, VideoId={EditingHistory.VideoId}");

            var created = await _adminHistoryService.CreateHistoryAsync(EditingHistory);

            if (created == null)
            {
                ErrorMessage = "Failed to create history. Check logs or server response.";
                Console.WriteLine("[DEBUG] CreateHistoryAsync returned null!");
                return;
            }

            Histories.Add(created);
            Console.WriteLine($"[DEBUG] History added! New count: {Histories.Count}");

            EditingHistory = new HistoryAdmin();

            // reload history
            await LoadHistoriesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating history: {ex.Message}";
            Console.WriteLine($"[DEBUG] Exception: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }




    // ------------------------------------------------------------------
    // Update existing entry
    // ------------------------------------------------------------------
    /// <summary>
    /// Updates an existing history entry using the data from EditingHistory.
    /// Validates the entry before sending to server and updates the Histories collection on success.
    /// </summary>
    [RelayCommand]
    public async Task UpdateHistoryAsync()
    {
        ErrorMessage = string.Empty;
        InfoMessage = string.Empty;

        try
        {
            IsLoading = true;

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

            EditingHistory.ValidateAll();
            if (EditingHistory.HasErrors)
            {
                // Abort if invalid
                return;
            }

            bool success = await _adminHistoryService.UpdateHistoryAsync(EditingHistory);
            if (!success)
            {
                ErrorMessage = "Failed to update history.";
                return;
            }

            // Update in list
            var index = Histories
                .ToList()
                .FindIndex(h => h.Id == EditingHistory.Id);

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

            // reload history
            await LoadHistoriesAsync();
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
    // Delete entry
    // ------------------------------------------------------------------
    /// <summary>
    /// Deletes the specified history entry from both server and local collection.
    /// </summary>
    /// <param name="history">The history entry to delete</param>
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
    // Edit selected entry
    // ------------------------------------------------------------------
    /// <summary>
    /// Prepares a history entry for editing by copying its data to EditingHistory
    /// and synchronizing the user and video selection dropdowns.
    /// </summary>
    /// <param name="history">The history entry to edit</param>
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

        SelectedUserId = history.UserId;
        SelectedVideoId = history.VideoId;
    }
}