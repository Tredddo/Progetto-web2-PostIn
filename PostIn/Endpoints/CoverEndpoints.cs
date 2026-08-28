namespace PostIn.Endpoints;

public static class CoverEndpoints
{
    public static IEndpointRouteBuilder MapCoverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/uploads/covers/{fileName}", (string fileName, IWebHostEnvironment env) =>
        {
            var safeFileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(env.ContentRootPath, "Uploads", "Covers", safeFileName);

            if (!File.Exists(filePath))
            {
                return Results.NotFound();
            }

            var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return Results.File(filePath, contentType);
        }).RequireAuthorization();

        return endpoints;
    }
}