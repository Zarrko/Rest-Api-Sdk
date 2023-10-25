// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using _2Movies.Contracts.Requests.V1;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using RestApiSdk;
using RestApiSdkConsumer;

var services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async (_, _) => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(x =>
        x.BaseAddress = new Uri("https://localhost:7070"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("mambo-jumbo-2021");

var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Spiderman 2",
    YearOfRelease = 2002,
    Genres = new []{ "Action"}
});

await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest()
{
    Title = "Spiderman 2",
    YearOfRelease = 2002,
    Genres = new []{ "Action", "Adventure"}
});

await moviesApi.DeleteMovieAsync(newMovie.Id);

var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
};

var movies = await moviesApi.GetMoviesAsync(request);

foreach (var movieResponse in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}