using LibraryAPI.PatternOnOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/configuration")]
    public class ConfigurationController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ProccesingPayments processPayment;
        private readonly IConfigurationSection section_01;
        private readonly IConfigurationSection section_02;
        private readonly PersonOptions _personOptions;

        public ConfigurationController(IConfiguration configuration,
            //IOptions<PersonOptions> personOptions // usefull for values that do not change
            IOptionsSnapshot<PersonOptions> personOptions, // it is updated for each request for scope services
            ProccesingPayments processPayment
            )
        {
            this.configuration = configuration;
            this.processPayment = processPayment;
            section_01 = configuration.GetSection("section_01");
            section_02 = configuration.GetSection("section_02");
            _personOptions = personOptions.Value;
        }

        [HttpGet("options-monitor")]
        public ActionResult GetRates()
        {
            return Ok(processPayment.GetRates());
        }

        [HttpGet("section_1_options")]
        public ActionResult GetSectionOptions()
        {
            return Ok(_personOptions);
        }

        [HttpGet("providers")]
        public ActionResult GetProvider()
        {
            var value = configuration.GetValue<string>("who_ami");
            return Ok(new { value });
        }

        [HttpGet("retrieveall")]
        public ActionResult GetRetrieveAll()
        {
            var children = configuration.GetChildren().Select(x => $"{x.Key}: {x.Value}");
            return Ok(new { children });
        }

        [HttpGet("section_01")]
        public ActionResult GetSection01()
        {
            var name = section_01.GetValue<string>("name");
            var age = section_01.GetValue<int>("age");

            return Ok(new { name, age });
        }

        [HttpGet("section_02")]
        public ActionResult GetSection02()
        {
            var name = section_02.GetValue<string>("name");
            var age = section_02.GetValue<int>("age");

            return Ok(new { name, age });
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var option1 = configuration["LastName"];
            var option2 = configuration.GetValue<string>("LastName")!;

            return option2;
        }

        [HttpGet("sections")]
        public ActionResult<string> GetSection()
        {
            var option1 = configuration["ConnectionStrings:DefaultConnection"];
            var option2 = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            var section = configuration.GetSection("ConnectionStrings")!;
            var option3 = section["DefaultConnection"];

            return option3!;
        }
    }
}
