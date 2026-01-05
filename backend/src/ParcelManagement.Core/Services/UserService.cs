using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Model.User;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IUserService
    {
        Task<User> UserRegisterAsync(string username, string password, string email, string unitName);

        Task<User> ParcelRoomManagerRegisterAsync(string username, string password, string email);

        Task<List<string>> UserLoginAsync(UserLoginRequest loginRequest);

        Task<User> GetUserById(Guid id);

        Task<IReadOnlyList<Parcel?>> GetParcelsByUserAsync(Guid userId, ParcelStatus? parcelStatus);

        Task<string> GetUserRole(Guid userId);

        Task<(IReadOnlyList<User>, int count)> GetUserForViewAsync(FilterPaginationRequest<UserSortableColumn> filter);
    }

    public class UserService(
        IUserRepository userRepository,
        IUserResidentUnitRepository userResidentUnitRepo,
        IResidentUnitRepository residentUnitRepo, 
        IParcelRepository parcelRepo, 
        INotificationPrefService npService
        ) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserResidentUnitRepository _userResidentUnitRepo = userResidentUnitRepo;
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepo;
        private readonly IParcelRepository _parcelRepo = parcelRepo;
        private readonly INotificationPrefService _npService = npService;
        public async Task<List<string>> UserLoginAsync(UserLoginRequest loginRequest)
        {
            var userByUsernameSpec = new UserByUsernameSpecification(loginRequest.Username);
            var possibleUser = await _userRepository.GetOneUserBySpecification(userByUsernameSpec) ?? 
                throw new InvalidCredentialException($"Invalid login credential - username");

            var isCredentialValid = PasswordService.VerifyPassword(possibleUser, possibleUser.PasswordHash, loginRequest.Password);
            if (!isCredentialValid)
            {
                throw new InvalidCredentialException("Invalid login credentials");
            }
            
            var userRole = possibleUser.Role;
            if (possibleUser.Role == UserRole.Resident)
            {
                await _npService.EnsureUserHasNotificationPref(possibleUser.Id);
            }

            possibleUser.RefreshToken = loginRequest.RefreshToken;
            possibleUser.RefreshTokenExpiry = loginRequest.RefreshTokenExpiry;

            await _userRepository.UpdateUserAsync(possibleUser);
            return [possibleUser!.Id.ToString(), userRole.ToString()];
        }

        // this service is only to register resident 
        public async Task<User> UserRegisterAsync(string username, string password, string email, string unitName)
        {
            var specByResidentUnit = new ResidentUnitByUnitNameSpecification(unitName);
            var realResidentUnit = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specByResidentUnit) ??
                throw new NullReferenceException($"Resident unit {unitName} not found");
            var userByUsernameSpec = new UserByUsernameSpecification(username);
            var existingUser = await _userRepository.GetOneUserBySpecification(userByUsernameSpec);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with username {username} has already exist.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                ResidentUnitDeprecated = "",
                PasswordHash = "will do this later",
                PasswordSalt = "will do this later",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var hashedPassword = PasswordService.HashPassword(newUser, password);
            newUser.PasswordHash = hashedPassword;
            var theNewUser = await _userRepository.CreateUserAsync(newUser);
            await _userResidentUnitRepo.CreateUserResidentUnitAsync(
                new UserResidentUnit
                {
                    Id = Guid.NewGuid(),
                    UserId = newUser.Id,
                    ResidentUnitId = realResidentUnit.Id,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            );

            await _npService.EnsureUserHasNotificationPref(newUser.Id);
            return theNewUser;
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id) ?? throw new KeyNotFoundException($"User with id {id} is not found");
        }

        public Task<IReadOnlyList<Parcel?>> GetParcelsByUserAsync(Guid userId, ParcelStatus? parcelStatus)
        {
            throw new NotImplementedException();
        }

        public async Task<User> ParcelRoomManagerRegisterAsync(string username, string password, string email)
        {
            var usernameSpec = new UserByUsernameSpecification(username);
            var existingUser = await _userRepository.GetOneUserBySpecification(usernameSpec);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with username {username} has alredy exist");
            }
            var registeringUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = "####",
                CreatedAt = DateTimeOffset.UtcNow, 
                Role = UserRole.ParcelRoomManager
            };
            registeringUser.PasswordHash = PasswordService.HashPassword(registeringUser, password);
            await _userRepository.CreateUserAsync(registeringUser);
            return registeringUser;
        }

        public async Task<string> GetUserRole(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId) ??
                throw new KeyNotFoundException($"User not found");
            return user.Role.ToString();
        }

        public async Task<(IReadOnlyList<User>, int count)> GetUserForViewAsync(FilterPaginationRequest<UserSortableColumn> filter)
        {
            var specification = new UserForViewSpecification(filter);
            filter.Page = null;
            filter.Take = null;
            var users = await _userRepository.GetUsersBySpecificationAsync(specification);
            var count = await _userRepository.GetUsersCountBySpecification(specification);
            return (users, count);
        }
    }
}