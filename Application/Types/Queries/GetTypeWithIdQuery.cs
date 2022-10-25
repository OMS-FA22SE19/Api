using Application.Common.Exceptions;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Types.Queries
{
    public sealed class GetTypeWithIdQuery : IRequest<Response<TypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetTypeWithIdQueryHandler : IRequestHandler<GetTypeWithIdQuery, Response<TypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTypeWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TypeDto>> Handle(GetTypeWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == request.Id);
            if (result is null)
            {
                throw new NotFoundException(nameof(Core.Entities.Type), request.Id);
            }
            var mappedResult = _mapper.Map<TypeDto>(result);
            return new Response<TypeDto>(mappedResult);
        }
    }
}
