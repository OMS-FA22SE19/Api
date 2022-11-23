using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class RefreshTokenRepository : EntityRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IApplicationDbContext context) : base(context, context.RefreshTokens)
    {
    }
}
}
