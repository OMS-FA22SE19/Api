﻿using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Menus.Commands
{
    public sealed class UpdateMenuCommand : IMapFrom<Menu>, IRequest<Response<MenuDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength(1000, MinimumLength = 2)]
        public string Description { get; set; }
        public bool Available { get; set; }

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
            var entity = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }

            MapToEntity(request, entity);
            if (request.Available)
            {
                var filter = new List<Expression<Func<Menu, bool>>>
                {
                    e => e.Available && !e.IsDeleted
                };
                var availableMenus = await _unitOfWork.MenuRepository.GetAllAsync(filter);
                foreach (var menu in availableMenus)
                {
                    menu.Available = false;
                    await _unitOfWork.MenuRepository.UpdateAsync(menu);
                }
            }
            var result = await _unitOfWork.MenuRepository.UpdateAsync(entity);
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

        private void MapToEntity(UpdateMenuCommand request, Menu entity)
        {
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Available = request.Available;
        }
    }
}
