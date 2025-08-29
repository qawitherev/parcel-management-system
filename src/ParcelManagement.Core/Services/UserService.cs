using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IUserService
    {
        Task<User> UserRegisterAsync(string username, string password, string email, string unitName);

        Task<List<string>> UserLoginAsync(string username, string password);

        Task<User?> GetUserById(Guid id);

        Task<IReadOnlyList<Parcel?>> GetParcelsByUserAsync(Guid userId, ParcelStatus? parcelStatus);
    }

    public class UserService(
        IUserRepository userRepository,
        IUserResidentUnitRepository userResidentUnitRepo,
        IResidentUnitRepository residentUnitRepo, 
        IParcelRepository parcelRepo
        ) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserResidentUnitRepository _userResidentUnitRepo = userResidentUnitRepo;
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepo;
        private readonly IParcelRepository _parcelRepo = parcelRepo;
        public async Task<List<string>> UserLoginAsync(string username, string password)
        {
            var userByUsernameSpec = new UserByUsernameSpecification(username);
            var possibleUser = await _userRepository.GetOneUserBySpecification(userByUsernameSpec) ?? throw new InvalidCredentialException($"Invalid login credential - username");
            var isCredentialValid = PasswordService.VerifyPassword(possibleUser, possibleUser.PasswordHash, password);
            if (!isCredentialValid)
            {
                throw new InvalidCredentialException("Invalid login credentials");
            }
            var userRole = possibleUser.Role;
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
            return theNewUser;
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id) ?? throw new KeyNotFoundException($"User with id {id} is not found");
        }

        public Task<IReadOnlyList<Parcel?>> GetParcelsByUserAsync(Guid userId, ParcelStatus? parcelStatus)
        {
            throw new NotImplementedException();
        }
    }
}