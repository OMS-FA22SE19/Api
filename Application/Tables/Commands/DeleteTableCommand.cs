using Application.Tables.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Tables.Commands
{
    public sealed class DeleteTableCommand : IRequest<Response<TableDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Response<TableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableDto>> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TableRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<TableDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

