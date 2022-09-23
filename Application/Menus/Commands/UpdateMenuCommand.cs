using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Commands
{
    public sealed class UpdateMenuCommand : IMapFrom<Menu>, IRequest<Response<MenuDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; }
        public bool IsHidden { get; set; } = true;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateMenuCommand, Menu>();
        }
    }

    public sealed class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }
            var updatedEntity = _mapper.Map<Menu>(request);

            var result = await _unitOfWork.MenuRepository.UpdateAsync(updatedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<MenuDto>("error");
            }
            var mappedResult = _mapper.Map<MenuDto>(result);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
