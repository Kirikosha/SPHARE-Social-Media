using Application.Core;

namespace Application.Errors;

public static class SpamErrors
{
    public static Error PublicationSpam => new ("You cannot create anymore publications due to our anti-spam " +
        "regulations", 403);
}