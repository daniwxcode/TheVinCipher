namespace Services.Interfaces
{
    public interface ICrudServices
    {
        public Task Ajouter (string element);
        public IEnumerable<string> ListeElements ();
        public string lienDeRecherche (string element);

    }


}
