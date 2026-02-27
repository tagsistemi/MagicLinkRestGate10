using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MagicLinkRestGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMagicLinkRestGateController : ControllerBase
    {
        private readonly MlkSettings _mlkSettings;

        public TestMagicLinkRestGateController(IOptions<MlkSettings> mlsettings)
        {
            _mlkSettings = mlsettings.Value;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[]
            {
                $"Login Server Mago: {_mlkSettings.LoginServerMago}",
                $"Istanza di Mago: {_mlkSettings.LoginInstallationName}",
                $"Azienda mago: {_mlkSettings.LoginCompany}"
            };
        }
    }
}
