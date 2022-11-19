using Application.Common.Exceptions;
using Application.Menus.Response;
using Application.Common.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Commands
{
    public sealed class RecoverMenuCommand : IRequest<Response<MenuDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverMenuCommandHandler : IRequestHandler<RecoverMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverMenuCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<MenuDto>> Handle(RecoverMenuCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }
            await _unitOfWork.MenuRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
