using Application.Common.Exceptions;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Type = Core.Entities.Type;

namespace Application.Types.Commands
{
    public sealed class RecoverTypeCommand : IRequest<Response<TypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverTypeCommandHandler : IRequestHandler<RecoverTypeCommand, Response<TypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<TypeDto>> Handle(RecoverTypeCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(Type), request.Id);
            }
            await _unitOfWork.TypeRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<TypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
