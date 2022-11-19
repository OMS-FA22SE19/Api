using Application.Common.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Types.Commands
{
    public sealed class DeleteTypeCommand : IRequest<Response<TypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteTypeCommandHandler : IRequestHandler<DeleteTypeCommand, Response<TypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TypeDto>> Handle(DeleteTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TypeRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<TypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
