using Application.Common.Mappings;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Types.Commands
{
    public sealed class CreateTypeCommand : IMapFrom<Core.Entities.Type>, IRequest<Response<TypeDto>>
    {
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTypeCommand, Core.Entities.Type>();
        }
    }

    public sealed class CreateTypeCommandHandler : IRequestHandler<CreateTypeCommand, Response<TypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TypeDto>> Handle(CreateTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Core.Entities.Type>(request);
            var result = await _unitOfWork.TypeRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TypeDto>("error");
            }
            var mappedResult = _mapper.Map<TypeDto>(result);
            return new Response<TypeDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
