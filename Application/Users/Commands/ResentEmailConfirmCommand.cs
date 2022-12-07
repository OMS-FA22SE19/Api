using Application.Common.Behaviours;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Users.Commands
{
    public class ResentEmailConfirmCommand : IMapFrom<ApplicationUser>, IRequest<Response<UserDto>>
    {
        [Required]
        public string email { get; set; }
    }

    public class ResentEmailConfirmCommandHandler : IRequestHandler<ResentEmailConfirmCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISendMailService _sendMailService;

        public ResentEmailConfirmCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ISendMailService sendMailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _sendMailService = sendMailService;
        }

        public async Task<Response<UserDto>> Handle(ResentEmailConfirmCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.email);
            if (user == null)
            {
                throw new NotFoundException("Can not find user");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var url = $"https://localhost:7246/api/v1/Users/ConfirmEmail/confirm?email=" + request.email + "&code=" + code;
            var realUrl = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(url);
            realUrl = realUrl.Replace('-', '+');
            realUrl = realUrl.Replace('_', '/');
            var content = new MailContent()
            {
                To = request.email,
                Subject = "Access information to OMS",
                Body = CreateBodyMessage(code, realUrl)
            };
            await _sendMailService.SendMail(content);

            var mappedResult = _mapper.Map<UserDto>(user);
            return new Response<UserDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        private string CreateBodyMessage(string code, string url)
        {
            return "<div lang=\"EN-US\" link=\"blue\" vlink=\"#954F72\">" +
                "<div class=\"m_-863817368153641209WordSection1\">" +
                "<p style=\"line-height:150%\"><span style=\"font-size:13.5pt;line-height:150%;color:black\">Quý khách vui lòng ấn vào đường link này để xác nhận:" + url + " </span><b></p>" +
                "</div>" +
                "</div>";
        }
    }
}
