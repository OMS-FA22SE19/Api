using Application.Common.Exceptions;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
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
        private readonly IMediator _mediator;

        public DeleteMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Response<MenuDto>> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }
            if (entity.Available)
            {
                var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id != request.Id && !e.IsDeleted);
                if (menu is null)
                {
                    throw new BadRequestException("You cannot disable this menu");
                }
                menu.Available = true;
                await _unitOfWork.MenuRepository.UpdateAsync(menu);
            }
            entity.Available = false;
            entity.IsDeleted = true;
            var result = await _unitOfWork.MenuRepository.UpdateAsync(entity);

            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

