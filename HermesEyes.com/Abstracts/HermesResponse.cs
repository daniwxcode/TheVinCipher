using Domaine.Abstracts;


namespace HermesEyes.com.Abstracts
{
    public abstract class HermesResponse<T> where T : BaseEntity
    {
        public DateTime ResponseDate { get; init; } = DateTime.Now;
        public string Message { get; set; }
        public bool Success { get; set; } = false;
        public T Data { get; set; }


    }
}
