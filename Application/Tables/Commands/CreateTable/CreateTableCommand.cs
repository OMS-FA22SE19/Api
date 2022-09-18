using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;

namespace Application.Tables.Commands.CreateTable
{
    public record CreateTableCommand : IRequest<int>, IMapFrom<Table>
    {
        public int NumOfSeats { get; init; }
        public TableStatus Status { get; init; }
        public TableType Type { get; init; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTableCommand, Table>();
        }
    }

    public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Table>(request);

            await _unitOfWork.TableRepository.InsertAsync(entity);

            await _unitOfWork.CompleteAsync(cancellationToken);

            return entity.Id;
        }
    }
}
