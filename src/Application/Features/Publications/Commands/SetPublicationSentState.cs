namespace Application.Features.Publications.Commands;

using Infrastructure;
using System.Threading;

public class SetPublicationSentState
{
    public class Query : IRequest
    {
        public int Id { get; set; }
        public bool State { get; set; }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query>
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications.FindAsync(request.Id);
            if (publication == null) throw new Exception("Publication was not found");

            publication.WasSent = request.State;
            context.Publications.Update(publication);
        }
    }
}
