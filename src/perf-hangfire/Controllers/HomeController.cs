using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Hangfire;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

using perf_hangfire.Models;

namespace perf_hangfire.Controllers
{

    public class HomeController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HomeController(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public IActionResult Index()
        {
            var peopleDAO = new PeopleDAO();
            ViewBag.count = peopleDAO.GetCount();
            ViewBag.hashcount = peopleDAO.GetCountHashes();

            return View();
        }

        public IActionResult People()
        {
            ViewBag.people = new PeopleDAO().GetPeople(10000);

            return View();
        }

        [HttpPost]
        public IActionResult Index(ProcessDTO process)
        {
            Start(process.batchsize);

            return Redirect("~/");
        }

        [HttpPost]
        public IActionResult clearhashes()
        {
            new PeopleDAO().ClearHashes();

            return Redirect("~/");
        }

        public void Start(int batchsize)
        {
            PeopleDAO peopleDAO = new PeopleDAO();
            var allPeople = peopleDAO.GetPeople(9999999);

            for (int i=0;i<allPeople.Count;i+= batchsize)
            {
                _backgroundJobClient.Enqueue(() => Process(i, batchsize));
            }
        }

        public void Process(int startFrom, int batchSize)
        {
            PeopleDAO peopleDAO = new PeopleDAO();

            using (var md5 = MD5.Create())
            {
                var batchOfPeople = peopleDAO.GetPeople(batchSize, startFrom);

                foreach (var person in batchOfPeople)
                {
                    string name = $"{person.FirstName} {person.LastName}";

                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(name);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    string hash = Convert.ToBase64String(hashBytes);

                    peopleDAO.UpdateHash(person.PersonID, hash);
                }
            }

        }

    }

}
