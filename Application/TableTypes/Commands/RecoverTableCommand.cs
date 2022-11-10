using Application.Common.Exceptions;
using Application.Models;
using Application.TableTypes.Response;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.TableTypes.Commands
{
    public sealed class RecoverTableTypeCommand : IRequest<Response<TableTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverTableTypeCommandHandler : IRequestHandler<RecoverTableTypeCommand, Response<TableTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverTableTypeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<TableTypeDto>> Handle(RecoverTableTypeCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(TableType), request.Id);
            }
            await _unitOfWork.TableTypeRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<TableTypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
