using ArticlesWebApp.Api.Common;

namespace ArticlesWebApp.Api.Entities;

public class AuthEventsEntity(
        Guid userId,
        AuthEvents eventType,
        bool isSucceeded)
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; } = userId;
    public AuthEvents EventType { get; init; } = eventType;
    public bool IsSucceeded { get; init; } = isSucceeded;
    public DateTime EventTime { get; init; } = DateTime.Now;
    //public string? EventString { get; init; }
    // left here. нужно придумать как записывать сюда регистрацию,
    // ибо юзера ещё не существует чтоб взять юзерайди
    // наверное, стоит сделать временное айди для всех новых пользователей без аккаунта
}
