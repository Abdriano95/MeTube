using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MeTube.Client.ViewModels.HistoryViewModels
{
    [ObservableObject]
    public partial class UserHistoryViewModel
    {
        private readonly IHistoryService _historyService;
        private readonly NavigationManager _navigationManager;
        private readonly IVideoService _videoService;
        private IJSRuntime _jsRuntime;

        [ObservableProperty]
        private List<History> _userHistory = new();

        public UserHistoryViewModel(IHistoryService historyService,
            NavigationManager navigationManager, 
            IVideoService videoService, 
            IJSRuntime jsRuntime)
        {
            _historyService = historyService;
            _navigationManager = navigationManager;
            _videoService = videoService;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadUserHistoryAsync()
        {
            try
            {
                UserHistory.Clear();
                var history = await _historyService.GetUserHistoryAsync();
                if (history != null)
                {
                    foreach (var item in history.OrderByDescending(h => h.DateWatched))
                    {
                        UserHistory.Add(item);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load history. Please try again.");
            }
        }


    }
}
