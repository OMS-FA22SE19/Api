using Application.Common.Mappings;
using Core.Entities;

namespace Application.Tables.Response
{
    public class TableByNumOfSeatDto : IMapFrom<Table>
    {
        public int NumOfSeats { get; set; }
        public int Total { get; set; }
        public IList<int> TableIds { get; set; }
    }
}
