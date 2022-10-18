using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Type = Core.Entities.Type;

namespace Application.Types.Commands
{
    public sealed class UpdateTypeCommand : IMapFrom<Core.Entities.Type>, IRequest<Response<TypeDto>>
    {
        [Required]
        public int Id { get; init; }
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateTypeCommand, Core.Entities.Type>();
        }
    }

    public sealed class UpdateTypeCommandHandler : IRequestHandler<UpdateTypeCommand, Response<TypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TypeDto>> Handle(UpdateTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Type), request.Id);
            }

            MapToEntity(request, entity);

            var result = await _unitOfWork.TypeRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TypeDto>("error");
            }
            var mappedResult = _mapper.Map<TypeDto>(result);
            return new Response<TypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private static void MapToEntity(UpdateTypeCommand request, Type? entity) => entity.Name = request.Name;
    }
}
