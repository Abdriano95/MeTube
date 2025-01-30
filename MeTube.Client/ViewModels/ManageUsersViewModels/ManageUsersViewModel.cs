using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.ManageUsersViewModels
{
    public partial class ManageUsersViewModel : ObservableValidator
    {
        private readonly IUserService _userService;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        private string search = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string userRole = string.Empty;

        [ObservableProperty]
        public string selectedRole = string.Empty;
        public ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> FilteredUsers { get; set; } = new ObservableCollection<User>();
        public ManageUsersViewModel(IUserService userService, IJSRuntime jsRuntime) 
        {
            _userService = userService;
            _jsRuntime = jsRuntime;
        }
        public async Task LoadUsers()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            AllUsers.Clear();
            foreach (var user in allUsers.OrderBy(a => a.Username))
            {
                AllUsers.Add(user);
                FilteredUsers.Add(user);
            }
        }
        private async Task<int> GetUserId(User user)
        {
            var hasse = await _userService.GetUserIdByEmailAsync(user.Email);
            return hasse.Value;
        }

        public async Task DeleteUserButton(User user)
        {
            int userId = await GetUserId(user);
            bool securedelete = await _jsRuntime.InvokeAsync<bool>("confirm", $"You sure you want to delete this user?");
            if (securedelete)
            {
                bool response = await _userService.DeleteUserAsync(userId);
                if (response)
                    await _jsRuntime.InvokeAsync<bool>("alert", "User succesfully deleted!");
                else
                    await _jsRuntime.InvokeAsync<bool>("alert", "Unable to succesfully delete user!");
            }
        }

        public async Task SaveChangesButton(User user)
        {
            int userId = await GetUserId(user);
            UpdateUserDto dto = new UpdateUserDto
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                Role = SelectedRole,
            };
            bool secureupdate = await _jsRuntime.InvokeAsync<bool>("confirm", "You sure you want to update this user?");
            if (secureupdate)
            {
                bool response = await _userService.UpdateUserAsync(userId, dto);
                string message = string.Empty;
                if(response)
                    await _jsRuntime.InvokeAsync<bool>("alert", "User succesfully saved!");
                else
                    await _jsRuntime.InvokeAsync<bool>("alert", "Unable to succesfully update user!");
            }
        }
        public void SearchButton()
        {
            if (string.IsNullOrWhiteSpace(Search))
                ResetSearchedSongs();
            IEnumerable<User> result = FilteredUsers.Where(a => a.Username.ToLower().Contains(Search.ToLower()) || a.Email.ToLower().Contains(Search.ToLower()) || a.Password.ToLower().Contains(Search.ToLower())).Distinct();
            AllUsers.Clear();
            foreach (User user in result)
                AllUsers.Add(user);
        }

        private void ResetSearchedSongs()
        {
            AllUsers.Clear();
            foreach (User song in FilteredUsers.Distinct())
                AllUsers.Add(song);
            return;
        }
        public async Task<bool> ConfirmLeave()
        {
            return await _jsRuntime.InvokeAsync<bool>("confirm", "Do you wish to leave and lose the pending changes?");
        }
    }
}
