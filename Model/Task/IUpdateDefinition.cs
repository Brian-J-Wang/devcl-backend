using MongoDB.Driver;

public interface IUpdateDefintion<T> {
    public UpdateDefinition<T> GetUpdateDefinition();
}