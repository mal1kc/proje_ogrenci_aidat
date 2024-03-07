public interface IBaseDbModel
{
    public int Id { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public interface IBaseDbModelView : IBaseDbModel
{
}
