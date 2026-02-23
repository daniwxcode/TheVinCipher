using Infrastructure.APIs.Abstracts;
using Infrastructure.Contexts;

using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace HermesEyes.com.Model;



[Route("api/[controller]/[Action]/")]
[ApiController]
public class BackGroundTAsk : ControllerBase
{
        private  HermesContext _hermesContext { get; set; }
        private readonly ICarService _service;
        private readonly ICrudServices requestsbase;
        private readonly TokensProvider _tokensprovider;
        private readonly IHttpConsumtionServices _httpclient;
        private readonly BaseApiProvider _BaseApiProvider;
        public BackGroundTAsk (ICarService carService, ICrudServices services, TokensProvider tokensProvider, IHttpConsumtionServices http, BaseApiProvider baseApiProvider, HermesContext hermesContext)
        {
            _service = carService;
            requestsbase = services;
            _tokensprovider = tokensProvider;
            _httpclient = http;
            _BaseApiProvider = baseApiProvider;
            _hermesContext = hermesContext;
        }

    [HttpPost]
    public void RunRequestsReparationsReparation ()
        {
            var requests = _hermesContext.Requests.ToList();
            foreach (var request in requests)
            {
                if (request.Vin.Length == 17)
                {
                    var r = _httpclient.FindCar(request.Vin);
                    if(r != null)
                    {
                        _hermesContext.Requests.Remove(request);
                    }
                }
                
            }
            _hermesContext.SaveChanges();
        }
    
    [HttpPost]
    public async Task<ActionResult<int>> RundecodeUpdateBase ()
        {
            var lists = _hermesContext.Cars.ToList().Take(100);
            foreach(var list in lists)
            {
                try
                {
                    var c = _httpclient.FindCar(list.Vin);
                }catch(Exception ex)
                {
                    //continue
                }
                
                
            }
        return 1;
        }
    }

