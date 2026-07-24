using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectPhase3.Data;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectPhase3.Controllers
{
    //TODO: add your controller as a "primary constructor" param:
    //eg: public class CommonController(MyContextType myContext) 
    public class CommonController(LmsContext db) : Controller
    {

        
        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            
            var departments = db.Departments
            .Select(d => new
            {
                name = d.Departmentname,
                subject = d.Subjectabbreviation
            }).ToArray();
           
            return Json(departments);
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var departments = db.Departments
                .Select(d => new
                {
                    subject = d.Subjectabbreviation,
                    dname = d.Departmentname,
                    courses = d.Courses
                        .Select(c => new
                        {
                            number = c.Coursenumber,
                            cname = c.Coursename
                        }).ToArray()
                })
                .ToArray();
            
            return Json(departments);
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var classes = db.Classes
                .Join(db.Courses,
                    c => c.Catalogid,
                    co => co.Catalogid,
                    (c, co) => new { classobj = c, course = co })
                .Join(db.Users,
                    combo => combo.classobj.Professorid,
                    u => u.Userid,
                    (combo, u) => new { combo, professor = u })
                .Where(p => p.combo.course.Subjectabbreviation == subject &&
                            p.combo.course.Coursenumber == number)
                .ToArray()  // Execute query first
                .Select(p =>
                {
                    var semesterPart = (p.combo.classobj.Semester ?? "")
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return new
                    {
                        season = semesterPart.Length > 0 ? semesterPart[0] : "",
                        year = semesterPart.Length > 1 ? semesterPart[1] : "",
                        location = p.combo.classobj.Location,
                        start = p.combo.classobj.Starttime.ToString(),
                        end = p.combo.classobj.Endtime.ToString(),
                        fname = p.professor.Firstname,
                        lname = p.professor.Lastname
                    };
                })
                .ToArray();
    
            return Json(classes);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var assignment = db.Assignments
                .FirstOrDefault(a =>
                    a.Assignmentname == asgname &&
                    a.Categorynames == category &&
                    a.Assignmentcategory != null &&
                    a.Assignmentcategory.Class.Semester == season + " " + year &&
                    a.Assignmentcategory.Class.Catalog.Subjectabbreviation == subject &&
                    a.Assignmentcategory.Class.Catalog.Coursenumber == num);

            if (assignment == null)
            {
                return Content("");
            }

            return Content(assignment.Content ?? "");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            if (string.IsNullOrWhiteSpace(uid) || !int.TryParse(uid.TrimStart('u', 'U'), out int userId))
            {
                return Content("");
            }
            var submission = db.Submissions
                .FirstOrDefault(s => 
                    s.Userid == userId && 
                    s.Assignment.Assignmentname == asgname &&
                    s.Assignment.Categorynames == category &&
                    s.Assignment.Assignmentcategory != null &&
                    s.Assignment.Assignmentcategory.Class.Semester == season + " " + year &&
                    s.Assignment.Assignmentcategory.Class.Catalog.Subjectabbreviation == subject &&
                    s.Assignment.Assignmentcategory.Class.Catalog.Coursenumber == num
                    );

            if (submission == null)
            {
                return Content("");
            }
            return Content(submission.Submissioncontents ?? "");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {      
            if (string.IsNullOrWhiteSpace(uid) || !int.TryParse(uid.TrimStart('u', 'U'), out int userId))
            {
                return Json(new { success = false, error = "" });
            }
            var user =  db.Users
                .FirstOrDefault(u => u.Userid == userId);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found" });
            }
            
            var professor = db.Professors
                .FirstOrDefault(p => p.Userid == user.Userid);
            if (professor != null)
            {
                return Json(new
                {
                    fname = user.Firstname,
                    lname = user.Lastname,
                    uid = "u" + user.Userid,
                    department = professor.SubjectabbreviationNavigation?.Departmentname
                });
            }
            
            var student = db.Students.FirstOrDefault(s => s.Userid == user.Userid);
            if (student != null)
            {
                return Json(new
                    {
                        fname = user.Firstname,
                        lname = user.Lastname,
                        uid = "u" + user.Userid,
                        department = student.SubjectabbreviationNavigation?.Departmentname
                    }
                );
            }
            
            var administrator = db.Administrators.FirstOrDefault(a => a.Userid == user.Userid);
            if (administrator != null)
            {
                return Json(new
                    {
                        fname = user.Firstname,
                        lname = user.Lastname,
                        uid = "u" + user.Userid,
                    }
                );
            }
            
                
            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

