namespace VinCipher.Model
{
    public class MarketValue
    {
        public int Value { get; set; }
        public string Currency { get; set; } = "XOF";
        public bool IsEvaluated { get; set; } = false;
        public MarketValue(int valeur)
        {
            Value = valeur;
            IsEvaluated = Value != 0;
        }
    }
}
