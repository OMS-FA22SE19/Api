using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Tables.Response;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Tables.Commands
{
    public sealed class RecoverTableCommand : IRequest<Response<TableDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverTableCommandHandler : IRequestHandler<RecoverTableCommand, Response<TableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverTableCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<TableDto>> Handle(RecoverTableCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.TableRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(Table), request.Id);
            }
            await _unitOfWork.TableRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<TableDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
