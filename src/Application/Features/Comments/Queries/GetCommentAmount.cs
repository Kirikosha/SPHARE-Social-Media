using Application.Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Comments.Queries;
public class GetCommentAmount
{
    public class Query : IRequest<Result<int>>
    {
        public int Id { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<int>>
    {
        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            var amount = context.Comments.Where(a => a.PublicationId == request.Id).Count();
            return Result<int>.Success(amount);
        }
    }
}
