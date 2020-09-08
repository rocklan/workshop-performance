using System;
using System.Collections.Generic;
using System.Linq;

namespace perf
{
    public class CountryLogic
    {
        private readonly PersonRepository _personRepository;

        // if the program is taking over 30 seconds to run set this to something lower
        private const int PeopleLimit = 10;


        public CountryLogic()
        {
            _personRepository = new PersonRepository();
        }

        public void WritePeopleOrderedByCountry(string outputFileName)
        {
            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }
          
            List<Country> countries = _personRepository.GetCountries();

            foreach (Country country in countries)
            {
                List<Person> people = _personRepository.GetPeopleFromCountry(country.CountryID, PeopleLimit);

                foreach (Person person in people)
                {
                    System.IO.File.AppendAllText(outputFileName, $"{country.CountryName} - {person.FirstName} {person.LastName}" + Environment.NewLine);
                }

                Console.WriteLine($"Written {people.Count} people who are in {country.CountryName}");
            }
        }

    }
}
