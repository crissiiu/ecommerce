using AuthenticationApi.Application.DTOs;
using eCommerce.SharedLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApi.Application.Interfaces
{
    public interface IUser
    {
        Task<Response> Login(LoginDTO loginDTO);
        Task<Response> Register(AppUserDTO appUserDTO);
        Task<GetUserDTO> GetUser(int UserId);
        //Task<Response> Logout(int UserId);
    }
}
