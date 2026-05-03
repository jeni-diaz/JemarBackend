using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;

namespace Jemar.Aplication.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<UserResponse> GetAll()
        {
            var users = _userRepository.GetAllAsync().Result;

            return UserMapper.ToListResponse(users);
        }


        public UserResponse? GetById(Guid id)
        {
            var user = _userRepository.GetByIdAsync(id).Result;

            if (user == null)
                return null;

            return UserMapper.ToResponse(user);
        }

        public UserResponse Create(CreateUserRequest request)
        {
            var user = UserMapper.ToEntity(request);

            _userRepository.AddAsync(user).Wait();

            return UserMapper.ToResponse(user);
        }

    }
}
