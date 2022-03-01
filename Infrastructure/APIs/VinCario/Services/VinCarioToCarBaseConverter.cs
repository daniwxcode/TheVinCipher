using Infrastructure.APIs.VinCario.Models;
using Infrastructure.Converters;

namespace Infrastructure.APIs.VinCario.Services
{
    public class VinCarioToCarBaseConverter : ResponseToCarDecoderBase<VinCarioResult>
    {
        public VinCarioToCarBaseConverter (VinCarioResult vinDecoderResult) : base(vinDecoderResult)
        {
            NewCar.Vin = getvaluefromResponse(vinDecoderResult, "VIN").ToString();
            NewCar.Make = getvaluefromResponse(vinDecoderResult, "Make").ToString();
            NewCar.MadeIn = getvaluefromResponse(vinDecoderResult, "Plant Country").ToString();
            NewCar.MadeInCity = getvaluefromResponse(vinDecoderResult, "Manufacturer Address").ToString();
            NewCar.Type = getvaluefromResponse(vinDecoderResult, "Product Type").ToString();
            NewCar.Model = getvaluefromResponse(vinDecoderResult, "Model").ToString();
            NewCar.Model += " " + getvaluefromResponse(vinDecoderResult, "Engine Model").ToString();
            NewCar.Model += " (" + getvaluefromResponse(vinDecoderResult, "Production Started").ToString();
            NewCar.Model += "-" + getvaluefromResponse(vinDecoderResult, "Production Stopped").ToString()+") ";

            NewCar.FuelType = getvaluefromResponse(vinDecoderResult, "Fuel Type - Primary").ToString();
            NewCar.Style = getvaluefromResponse(vinDecoderResult, "Body").ToString();
            int n = 0;
            NewCar.Doors= Int32.TryParse(getvaluefromResponse(vinDecoderResult, "Number of Doors").ToString(), out n)?n:0;
            NewCar.Year= Int32.TryParse(getvaluefromResponse(vinDecoderResult, "Model Year").ToString(), out n)?n:0;
            NewCar.WeightEmptykg = Int32.TryParse(getvaluefromResponse(vinDecoderResult, "Weight Empty (kg)").ToString(), out n)?n:0;
            NewCar.GrossVehicleWeightRating = getvaluefromResponse(vinDecoderResult, "Max Weight (kg)").ToString();
            NewCar.CurbWeight = getvaluefromResponse(vinDecoderResult, "Max Weight (kg)").ToString();
            NewCar.StandardSeating= Int32.TryParse(getvaluefromResponse(vinDecoderResult, "Number of Seats").ToString(), out n)?n:0;
            NewCar.Transmission= getvaluefromResponse(vinDecoderResult, "Transmission").ToString();
            NewCar.EngineCylinders= getvaluefromResponse(vinDecoderResult, "Engine Displacement (ccm)").ToString();
            NewCar.Engine = getvaluefromResponse(vinDecoderResult, "Engine Model").ToString();
            NewCar.EngineSize = getvaluefromResponse(vinDecoderResult,"Engine Power (kW)").ToString();
            NewCar.Size = getvaluefromResponse(vinDecoderResult, "Wheelbase(mm)").ToString();
            NewCar.WheelbaseLength= getvaluefromResponse(vinDecoderResult, "Wheel Size Array").ToString() ;
            NewCar.SequentialNumber = getvaluefromResponse(vinDecoderResult, "Sequential Number").ToString();
            NewCar.OverallHeight = getvaluefromResponse(vinDecoderResult, "Height (mm)").ToString();
            NewCar.OverallLength = getvaluefromResponse(vinDecoderResult, "Length (mm)").ToString();
            NewCar.OverallWidth = getvaluefromResponse(vinDecoderResult, "Width (mm)").ToString();
            NewCar.CityMileage= getvaluefromResponse(vinDecoderResult, "Fuel Consumption l/100km (Combined)").ToString();
            NewCar.TransmissionType = getvaluefromResponse(vinDecoderResult, "Transmission(full)").ToString();
            NewCar.SteeringType = getvaluefromResponse(vinDecoderResult, "Steering").ToString();
            NewCar.TransmissionSpeeds = getvaluefromResponse(vinDecoderResult, "Number of Axles").ToString();
            var Date = new DateTime();
            
            if (DateTime.TryParse(getvaluefromResponse(vinDecoderResult, "Made").ToString(),out Date)){
                NewCar.MadeDeate= Date;
            }
        }
        private Object getvaluefromResponse (VinCarioResult vinDecoderResult, string label)
        {
            var t = vinDecoderResult.decode.FirstOrDefault(p => String.Compare(p.label, label) == 0);
            if (t != null)
            {
                return t.value;
            }

            return String.Empty;

        }
        protected override string getRigthlabel (string label)
        {
            string response = label;
            switch (label)
            {
                case "MadeIn":
                    {
                        response = "Plant Country";
                        break;
                    }
                case "MadeInCity":
                    {
                        response = "Plant State";
                        break;
                    }
                case "Style":
                    {
                        response = "Body";
                        break;
                    }
                case "Doors":
                    {
                        response = "Number of Doors";
                        break;
                    }
                case "Drivetrain":
                    {
                        response = "Drive";
                        break;

                    }
                case "Year":
                    {
                        response = "Model Year";
                        break;
                    }
                case "MadeDeate":
                    {
                        response = "Made";
                        break;
                    }
                case "FuelType":
                    {
                        response = "Fuel Type - Primary";
                        break;
                    }
                case "SequentialNumber":
                    {
                        response = "Sequential Number";
                        break;
                    }
                case "EngineSize":
                    {
                        response = "Engine Displacement (ccm)";
                        break;
                    }
                case "Engine":
                    {
                        response = "Engine Model";
                        break;
                    }
                case "SteeringType":
                    {
                        response = "Steering";
                        break;
                    }
                case "WeightEmptykg":
                    {
                        response = "Weight Empty (kg)";
                        break;
                    }
                case "Transmission":
                    {
                        response = "Transmission (full)";
                        break;
                    }
                case "TransmissionType":
                    {
                        response = "Transmission";
                        break;
                    }


            }
            return response;
        }


    }
}

