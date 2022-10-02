using Application.Models;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.TableTypes.Commands
{
    public sealed class DeleteTableTypeCommand : IRequest<Response<TableTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteTableTypeCommandHandler : IRequestHandler<DeleteTableTypeCommand, Response<TableTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteTableTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableTypeDto>> Handle(DeleteTableTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TableTypeRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<TableTypeDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

