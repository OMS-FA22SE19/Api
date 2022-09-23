﻿using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Commands
{
    public sealed class DeleteMenuCommand : IRequest<Response<MenuDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.MenuRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
