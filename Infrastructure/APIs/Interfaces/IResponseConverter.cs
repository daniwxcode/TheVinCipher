namespace Infrastructure.APIs.Interfaces
{
    public interface IResponseConverter<T> where T : class
    {
        public T GetT ();

    }
}
