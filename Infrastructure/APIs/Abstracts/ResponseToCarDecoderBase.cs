
using Domaine.Entities;

using Infrastructure.APIs.Interfaces;

namespace Infrastructure.Converters
{
    public abstract class ResponseToCarDecoderBase<T> : IResponseConverter<CarBase>
    {
        public CarBase NewCar { get; set; } = new CarBase();
        protected T ResponseToConvert { get; set; }

        protected ResponseToCarDecoderBase (T Response)
        {
            ResponseToConvert = Response;
        }

        public CarBase GetT ()
        {
            return new CarBase();
        }
        protected virtual string getRigthlabel (string label)
        {
            return label;
        }
    }
}