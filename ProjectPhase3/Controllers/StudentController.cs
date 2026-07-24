using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPhase3.Data;
using ProjectPhase3.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectPhase3.Controllers
{
    //TODO: add your controller as a "primary constructor" param:
    //eg: public class ProfessorController(MyContextType myContext) 
    // this means the whole controller is restricted to users with student role
    [Authorize(Roles = "Student")]
    // added primary constructor lmsContext db
    public class StudentController(LmsContext db) : Controller
    {

        // store context 
        private readonly LmsContext db = db;
        

        // view/Student/index.cshtml
        public IActionResult Index()
        {
            return View();
        }

        // view/Student/Catalog.cshtml
        public IActionResult Catalog()
        {
            return View();
        }

        // receives info from URL or Query String  
        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            Console.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            // we dont need to use varification because we already have     [Authorize(Roles = "Student")]

            // remove the u so we can check only number
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Json(Array.Empty<object>());
            }
            
            string numericPart = uid.TrimStart('u', 'U');

            
            // now we check only number
            if (!int.TryParse(numericPart, out int userID))
            {
                return Json(Array.Empty<object>());
            }

            // get table FROM student
            var student = db.Students
                .FirstOrDefault(s => s.Userid == userID);

            // handel empty
            if (student == null)
            {
                return Json(Array.Empty<object>());
            }

            // since model has enrollment we can user it without join
            // JOIN enrollemnt ON enrollemnt.userid = student.useris
            var enrollments = db.Enrollments
                .Where(e => e.Userid == student.Userid);

            // use Select to return values
            var values = enrollments
                .Select(e => new
                {
                    subject = e.Class.Catalog!.Subjectabbreviation,
                    number = e.Class.Catalog.Coursenumber,
                    name = e.Class.Catalog.Coursename,
                    // year display with semester EX. Fall 2026
                    semester = e.Class.Semester,
                    grade = e.Grade ?? "--"
                })
                .ToArray()
                .Select(e =>
                {
                    // handel semester and year 
                    string[] semesterParts =
                        (e.semester ?? "").Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries
                        );

                    return new
                    {
                        e.subject,
                        e.number,
                        e.name,
                        season = semesterParts.Length > 0
                            ? semesterParts[0]
                            : "",
                        year = semesterParts.Length > 1
                            ? semesterParts[1]
                            : "",
                        e.grade
                    };
                })
                .ToArray();

            return Json(values);
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            if (string.IsNullOrWhiteSpace(uid) || !int.TryParse(uid.TrimStart('u', 'U'), out int userID))
            {
                return Json(Array.Empty<object>());
            }

            string semester = season + " " + year;

            var classInfo = db.Classes.FirstOrDefault(c => 
                c.Catalog.Subjectabbreviation == subject &&
                c.Catalog.Coursenumber == num &&
                c.Semester == semester);

            if (classInfo == null)
            {
                return Json(Array.Empty<object>());
            }
            
            bool enrolled = db.Enrollments.Any(e => 
                e.Userid == userID && 
                e.Classid == classInfo.Classid);

            if (!enrolled)
            {
                return Json(Array.Empty<object>());
            }

            var assignments = db.Assignments
                .Where(a => a.Assignmentcategory != null &&
                            a.Assignmentcategory.Classid == classInfo.Classid).ToArray();

            var Submissions = db.Submissions
                .Where(s => s.Userid == userID &&
                            s.Classid == classInfo.Classid).ToArray();
            
            var result = assignments.Select(a =>
            {
                var submisstion =  Submissions.FirstOrDefault(s => s.Assignmentid == a.Assignmentid);

                return new
                {
                    aname = a.Assignmentname,
                    cname = a.Categorynames,
                    due = a.Duedate,
                    score = submisstion?.Score
                };
            }).ToArray();
            
            
            return Json(result);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, string category, string asgname, string uid, string contents)
        {           
            // 1. Safe input parsing
            if (string.IsNullOrWhiteSpace(uid) || !int.TryParse(uid.TrimStart('u', 'U'), out int userId))
            {
                return Json(new { success = false });
            }

            string semester = $"{season} {year}";

            try
            {
                // 2. Single-pass query: Validates assignment path AND confirms student enrollment
                var assignmentDetails = db.Assignments
                    .Where(a => a.Assignmentname == asgname &&
                                a.Categorynames == category &&
                                a.Assignmentcategory != null &&
                                a.Assignmentcategory.Class.Semester == semester &&
                                a.Assignmentcategory.Class.Catalog.Subjectabbreviation == subject &&
                                a.Assignmentcategory.Class.Catalog.Coursenumber == num &&
                                db.Enrollments.Any(e => e.Userid == userId && e.Classid == a.Assignmentcategory.Classid))
                    .Select(a => new 
                    {
                        AssignmentId = a.Assignmentid,
                        ClassId = a.Assignmentcategory.Classid
                    })
                    .FirstOrDefault();

                // If assignment path doesn't exist OR student isn't enrolled, reject immediately
                if (assignmentDetails == null)
                {
                    return Json(new { success = false });
                }

                // 3. Locate existing submission or upsert a new one
                var submission = db.Submissions.FirstOrDefault(s =>
                    s.Assignmentid == assignmentDetails.AssignmentId &&
                    s.Userid == userId);

                if (submission == null)
                {
                    // Initial submission setup
                    submission = new Submission
                    {
                        Assignmentid = assignmentDetails.AssignmentId,
                        Userid = userId,
                        Classid = assignmentDetails.ClassId,
                        Submissioncontents = contents,
                        Submissiontime = DateTime.Now,
                        Score = 0
                    };
                    db.Submissions.Add(submission);
                }
                else
                {
                    // Update submission criteria while preserving score
                    submission.Submissioncontents = contents;
                    submission.Submissiontime = DateTime.Now;
                }

                // 4. Save changes safely wrapped inside our try block
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                // Catches unexpected database dropouts, constraint failures, or connection timeouts
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            if (string.IsNullOrWhiteSpace(uid) || !int.TryParse(uid.TrimStart('u', 'U'), out int userId))
            {
                return Json(new { success = false });
            }

            bool studentExists = db.Students.Any(s => s.Userid == userId);
            if (!studentExists)
            {
                return Json(new { success = false });
            }
            
            string semester = season + " " + year;

            var classInfo = db.Classes.FirstOrDefault(c => 
                c.Catalog.Subjectabbreviation == subject &&
                c.Catalog.Coursenumber == num &&
                c.Semester == semester);

            if (classInfo == null)
            {
                return Json(new { success = false });
            }
            
            bool alreadyEnrolled = db.Enrollments.Any(e => e.Userid == userId && e.Classid == classInfo.Classid);
            if (alreadyEnrolled)
            {
                return Json(new { success = false });
            }
            
            var enrollment = new Enrollment
            {
                Userid = userId,
                Classid = classInfo.Classid,
                Grade = null
            };
            
            db.Enrollments.Add(enrollment);
            db.SaveChanges();
            
            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid) ||
                !int.TryParse(uid.TrimStart('u', 'U'), out int userId))
            {
                return Json(new { gpa = 0.0 });
            }

            var grades = db.Enrollments
                .Where(e =>
                    e.Userid == userId &&
                    e.Grade != null &&
                    e.Grade != "--")
                .Select(e => e.Grade!)
                .ToArray();

            if (grades.Length == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            var gradePoints = new Dictionary<string, double>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["A"]  = 4.0,
                ["A-"] = 3.7,
                ["B+"] = 3.3,
                ["B"]  = 3.0,
                ["B-"] = 2.7,
                ["C+"] = 2.3,
                ["C"]  = 2.0,
                ["C-"] = 1.7,
                ["D+"] = 1.3,
                ["D"]  = 1.0,
                ["D-"] = 0.7,
                ["E"]  = 0.0,
                ["F"]  = 0.0
            };

            var validPoints = grades
                .Where(g => gradePoints.ContainsKey(g.Trim()))
                .Select(g => gradePoints[g.Trim()])
                .ToArray();

            if (validPoints.Length == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            double gpa = validPoints.Average();

            return Json(new
            {
                gpa = Math.Round(gpa, 2)
            });
        }
                
        /*******End code to modify********/

    }
}

