using Application.Authentication.Models;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Response<AuthenticationResponse>> Authenticate(AuthenticationRequest request);
    }
}
