using Infrastructure.APIs.VinCario.Models;
using Infrastructure.Converters;

namespace Infrastructure.APIs.VinCario.Services
{
    public class VinCarioToCarBaseConverter : ResponseToCarDecoderBase<VinCarioResult>
    {
        public VinCarioToCarBaseConverter (VinCarioResult Response) : base(Response)
        {

            var p = GetType().GetField("CarBase")?.GetValue(this);
            foreach (var propertyInfo in CarBase.GetType().GetProperties())
            {
                Decode item = null;
                string rigthLabel = getRigthlabel(propertyInfo.Name);
                if (rigthLabel == propertyInfo.Name)
                {
                    item = Response.decode.FirstOrDefault(p => p.label.ToLower().Replace(" ", String.Empty) == propertyInfo.Name.ToLower());
                }
                else
                {
                    item = Response.decode.FirstOrDefault(p => String.Compare(p.label, rigthLabel) == 0);
                }
                if (item == null)
                {
                    continue;
                }

                try
                {
                    DateTime? date = null;
                    if (propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        date = Convert.ToDateTime(item.value);
                        propertyInfo.SetValue(CarBase, date);
                    }
                    else
                    {
                        if (propertyInfo.PropertyType == typeof(int?))
                        {
                            int fieldWithRightType = Convert.ToInt32(item.value);

                            propertyInfo.SetValue(CarBase, fieldWithRightType);
                        }
                        else
                        {
                            var fieldWithRightType = Convert.ChangeType(item.value, propertyInfo.PropertyType);

                            propertyInfo.SetValue(CarBase, fieldWithRightType);

                        }

                    }
                }

                catch (Exception ex)
                {
                    var msg = ex.Message;

                }

            }
            var l = getvaluefromResponse(Response, "Length (mm)");
            var w = getvaluefromResponse(Response, "Width (mm)");
            var h = getvaluefromResponse(Response, "Height (mm)");

            CarBase.Size = $"Length (mm):{l.ToString()}*Width (mm):{w}*Height (mm):{h}";
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

