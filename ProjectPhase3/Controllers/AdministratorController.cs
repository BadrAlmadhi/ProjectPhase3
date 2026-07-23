using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectPhase3.Data;
using ProjectPhase3.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectPhase3.Controllers
{
    //TODO: add your controller as a "primary constructor" param:
    //eg: public class AdministratorController(MyContextType myContext) 
    public class AdministratorController(LmsContext db) : Controller
    {

        private readonly LmsContext db;

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {

            bool alreadyExists = db.Departments
                .Any(d => d.Subjectabbreviation == subject);
            if (alreadyExists)
            {
                return Json(new { success = false });
            }

            var newDepartment = new Department
            {
                Subjectabbreviation = subject,
                Departmentname = name
            };
            
            db.Departments.Add(newDepartment);
            db.SaveChanges();
            
            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses = db.Courses
                .Where(c => c.Subjectabbreviation == subject);
            var values = courses
                .Select(c => new
                {
                    number = c.Coursenumber,
                    name = c.Coursename
                }).ToArray();
            
            return Json(values);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professors = db.Professors
                .Where(p => p.Subjectabbreviation == subject);
            var values = professors
                .Select(p => new
                {
                    lname = p.User.Lastname,
                    fname = p.User.Firstname,
                    uid = "u" + p.User.Userid
                }).ToArray();
            
            return Json(values);
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {          
            // fisrt check if course exsist 
            bool alreadyExists = db.Courses
                .Any(c => c.Subjectabbreviation == subject &&
                          c.Coursenumber == number);
            if (alreadyExists)
            {
                return Json(new { success = false });
            }

            var newCourse = new Course
            {
                Subjectabbreviation = subject,
                Coursenumber = number,
                Coursename = name
            };
            db.Courses.Add(newCourse);
            db.SaveChanges();
            
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            
            string semester = season + " " + year;
            // fix profosser id number without U
            string numericPart = instructor.TrimStart('u', 'U');
            if (!int.TryParse(numericPart, out int professorID))
            {
                return Json(new { success = false });
            }
            
            // return the course
            var courses = db.Courses.FirstOrDefault(c => 
                c.Subjectabbreviation == subject &&
                c.Coursenumber == number);

            if (courses != null)
            {
                return Json(new { success = false });
            }
            
            // check if course exsist 
            bool sameCourseExists = db.Courses.Any(c => 
                c.Subjectabbreviation == subject &&
                c.Coursenumber == number);

            if (sameCourseExists)
            {
                return Json(new { success = false });
            }
            
            bool locationConflict = db.Classes.Any(c =>
                c.Location == location &&
                c.Semester == semester &&
                c.Starttime < end &&
                c.Endtime > start);

            if (locationConflict)
            {
                return Json(new { success = false });
            }

            var newClass = new Class
            {
                Catalogid = courses.Catalogid,
                Semester = semester,
                Location = location,
                Starttime = start,
                Endtime = end,
                Professorid = professorID,
            };
            
            return Json(new { success = true});
        }


        /*******End code to modify********/

    }
}

