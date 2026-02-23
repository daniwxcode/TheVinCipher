using Domaine.Entities;

using Infrastructure.APIs.VinAudit.Models;
using Infrastructure.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.APIs.VinAudit.Services
{
    public class VinAuditToCarBaseConverter: ResponseToCarDecoderBase<VinAuditResponse>
    {
        public VinAuditToCarBaseConverter (VinAuditResponse VinAuditResponse):base(VinAuditResponse)
        {

            NewCar = new CarBase()
            {
                AntiBrakeSystem = VinAuditResponse.Attributes.AntiBrakeSystem,
                Category = VinAuditResponse.Attributes.Category,
                CityMileage = VinAuditResponse.Attributes.CityMileage,
                CreatedOn = DateTime.Now,
                CurbWeight = VinAuditResponse.Attributes.CurbWeight,
                Drivetrain = VinAuditResponse.Attributes.Drivetrain,
                EngineCylinders = VinAuditResponse.Attributes.EngineCylinders,
                FuelCapacity = VinAuditResponse.Attributes.FuelCapacity,
                GrossVehicleWeightRating = VinAuditResponse.Attributes.GrossVehicleWeightRating,
                FuelType = VinAuditResponse.Attributes.FuelType,
                Make = VinAuditResponse.Attributes.Make,
                EngineSize = VinAuditResponse.Attributes.EngineSize,
                MadeIn = VinAuditResponse.Attributes.MadeIn,
                WheelbaseLength = VinAuditResponse.Attributes.WheelbaseLength,
                Type = VinAuditResponse.Attributes.Type,
                TransmissionType = VinAuditResponse.Attributes.TransmissionType,
                TransmissionSpeeds = VinAuditResponse.Attributes.TransmissionSpeeds,
                Style = VinAuditResponse.Attributes.Style,
                SteeringType = VinAuditResponse.Attributes.SteeringType,
                Size = VinAuditResponse.Attributes.Size,
                OverallLength = VinAuditResponse.Attributes.OverallLength,
                OverallHeight = VinAuditResponse.Attributes.OverallHeight,
                Model = VinAuditResponse.Attributes.Model,
                MadeInCity = VinAuditResponse.Attributes.MadeInCity,
                HighwayMileage = VinAuditResponse.Attributes.HighwayMileage,
                Trim = VinAuditResponse.Attributes.Trim,
                Engine = VinAuditResponse.Attributes.Engine,
                ManufacturerSuggestedRetailPrice = VinAuditResponse.Attributes.ManufacturerSuggestedRetailPrice,
                MarketValue = 0
            };
            int t=0;
            int.TryParse(VinAuditResponse.Attributes.Doors, out t);
            NewCar.Doors = t;
            t = 0;
            int.TryParse(VinAuditResponse.Attributes.StandardSeating, out t);
            NewCar.StandardSeating = t;
            var p = GetType().GetField("NewCar")?.GetValue(this);
            foreach (var propertyInfo in NewCar.GetType().GetProperties())
            {
                var item = VinAuditResponse.GetType().GetProperties().FirstOrDefault(p => String.Compare(p.Name, propertyInfo.Name, true) == 0);
               
                if (item == null)
                {
                    continue;
                }

                DateTime? date = null;
                try
                {
                    if (propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        date = Convert.ToDateTime(item.GetValue(VinAuditResponse).ToString());
                        propertyInfo.SetValue(NewCar, date);
                    }
                    else
                    {
                        try
                        {
                            var fieldWithRightType = Convert.ChangeType(item.GetValue(VinAuditResponse), propertyInfo.PropertyType);
                            propertyInfo.SetValue(NewCar, fieldWithRightType);
                        }
                        catch (Exception)
                        {
                            //ignore
                        }



                    }
                }
                catch (Exception ex)
                {
                    var exc = ex.Message;
                }
            }

        }

    }
}
