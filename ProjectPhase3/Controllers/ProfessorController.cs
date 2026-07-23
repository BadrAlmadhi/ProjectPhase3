using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPhase3.Data;
using ProjectPhase3.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectPhase3.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController(LmsContext db) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students = db.Enrollments
                .Join(db.Classes,
                    enrollments => enrollments.Classid,
                    classes => classes.Classid,
                    (enrollments, classes) => new { enrollments, classes })
                .Join(db.Users,
                    combo => combo.enrollments.Userid,
                    users => users.Userid,
                    (users, combo) => new
                    { combo, users })
                .Join(db.Courses,
                    studentdata => studentdata.users.classes.Catalogid,
                    courses => courses.Catalogid,
                    (studentdata, courses) => new { studentdata, courses })
                .Where(p => p.courses.Coursenumber == num)
                .Where(p => p.courses.Subjectabbreviation == subject)
                .Where(p => p.studentdata.users.classes.Semester != null && p.studentdata.users.classes.Semester.StartsWith(season))
                .Where(p => p.studentdata.users.classes.Semester != null && p.studentdata.users.classes.Semester.EndsWith(year.ToString()))
                .Select(p => new
                {
                    fname = p.studentdata.combo.Firstname,
                    lname = p.studentdata.combo.Lastname,
                    uid = p.studentdata.combo.Userid,
                    dob = p.studentdata.combo.Dob,
                    grade = p.studentdata.users.enrollments.Grade
                }).ToArray();
            
            return Json(students);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var assignments = db.Assignments
                .Join(db.Classes,
                    assignments => assignments.Classid,
                    classes => classes.Classid,
                    (assignments, classes) => new { assignments, classes })
                .Join(db.Assignmentcategories,
                    combo => combo.assignments.Classid,
                    assignmentcats => assignmentcats.Classid,
                    (assignmentcats, combo) => new
                    { combo, assignmentcats })
                .Join(db.Courses,
                    assignmentdata => assignmentdata.assignmentcats.classes.Catalogid,
                    courses => courses.Catalogid,
                    (assignmentdata, courses) => new { assignmentdata, courses })
                .Join(db.Submissions,
                    fullassign => fullassign.assignmentdata.assignmentcats.assignments.Assignmentid,
                    subs => subs.Assignmentid,
                    (fullassign, subs) => new { fullassign, subs })
                .Where(p => p.fullassign.courses.Coursenumber == num)
                .Where(p => p.fullassign.courses.Subjectabbreviation == subject)
                .Where(p => p.fullassign.assignmentdata.assignmentcats.assignments.Assignmentcategory != null && p.fullassign.assignmentdata.assignmentcats.assignments.Assignmentcategory.Categorynames == category)
                .Where(p => p.fullassign.assignmentdata.assignmentcats.classes.Semester != null && p.fullassign.assignmentdata.assignmentcats.classes.Semester.StartsWith(season))
                .Where(p => p.fullassign.assignmentdata.assignmentcats.classes.Semester != null && p.fullassign.assignmentdata.assignmentcats.classes.Semester.EndsWith(year.ToString()))
                .Select(p => new
                {
                    aname = p.fullassign.assignmentdata.assignmentcats.assignments.Assignmentname,
                    cname = p.fullassign.assignmentdata.assignmentcats.assignments.Categorynames,
                    due = p.fullassign.assignmentdata.assignmentcats.assignments.Duedate,
                    submissions = p.fullassign.assignmentdata.assignmentcats.assignments.Submissions.GroupBy(p => p.Assignmentid).Count(),
                    
                }).ToArray();
            return Json(assignments);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignmentcats = db.Assignmentcategories
                .Join(db.Classes,
                    assignmentcat => assignmentcat.Classid,
                    classes => classes.Classid,
                    (assignments, classes) => new { assignments, classes })
                .Join(db.Courses,
                    combo => combo.classes.Catalogid,
                    courses => courses.Catalogid,
                    (combo, courses) => new
                    { combo, courses })
                .Where(p => p.courses.Coursenumber == num)
                .Where(p => p.courses.Subjectabbreviation == subject)
                .Where(p => p.combo.classes.Semester != null && p.combo.classes.Semester.StartsWith(season))
                .Where(p => p.combo.classes.Semester != null && p.combo.classes.Semester.EndsWith(year.ToString()))
                .Select(p => new
                {
                    cname = p.combo.assignments.Categorynames,
                    weight = p.combo.assignments.Gradingweight
                }).ToArray();
            return Json(assignmentcats);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classid = db.Courses
                .Join(db.Classes,
                    courses => courses.Catalogid,
                    classes => classes.Catalogid,
                    (courses, classes) => new { courses, classes })
                .Where(p => p.classes.Semester == (season + year))
                .Where(p => p.courses.Subjectabbreviation == subject)
                .Where(p => p.courses.Coursenumber == num)
                .Select(p => new
                {
                    classid = p.classes.Classid
                }).ToString();

            if (classid != null)
            {
                var newAssignmentCat = new Assignmentcategory
                {
                    Categorynames = category,
                    Gradingweight = catweight,
                    Classid = int.Parse(classid)
                };

                try
                {
                    db.Assignmentcategories.Add(newAssignmentCat);
                    db.SaveChanges();
                
                    return Json(new { success = true});
                }
                catch (DbUpdateException e)
                {
                    return Json(new { success = false });
                }
            }
            return Json(new { success = false });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var classid = db.Courses
                .Join(db.Classes,
                    courses => courses.Catalogid,
                    classes => classes.Catalogid,
                    (courses, classes) => new { courses, classes })
                .Where(p => p.classes.Semester == (season + year))
                .Where(p => p.courses.Subjectabbreviation == subject)
                .Where(p => p.courses.Coursenumber == num)
                .Select(p => new
                {
                    classid = p.classes.Classid
                }).ToString();

            if (classid != null)
            {
                var newAssignment = new Assignment
                {
                    Categorynames = category,
                    Maxpoint =  asgpoints,
                    Assignmentname = asgname,
                    Duedate = asgdue,
                    Content =  asgcontents,
                    Classid = int.Parse(classid)
                };

                try
                {
                    db.Assignments.Add(newAssignment);
                    db.SaveChanges();
                
                    return Json(new { success = true});
                }
                catch (DbUpdateException e)
                {
                    return Json(new { success = false });
                }
            }
            return Json(new { success = false });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var submissions = db.Assignments
                .Join(db.Submissions,
                    assignmentcat => assignmentcat.Assignmentid,
                    subs => subs.Assignmentid,
                    (assignments, subs) => new { assignments, subs })
                .Join(db.Classes,
                    combo => combo.subs.Classid,
                    classes => classes.Classid,
                    (combo, classes) => new
                        { combo, classes })
                .Join(db.Courses,
                    classinfo => classinfo.classes.Catalogid,
                    courses => courses.Catalogid,
                    (classinfo, courses) => new { classinfo, courses })
                .Join(db.Users,
                    assignmentdata => assignmentdata.classinfo.combo.subs.Userid,
                    students => students.Userid,
                    (assignmentdata, students) => new { assignmentdata, students })
                .Where(p => p.assignmentdata.classinfo.classes.Semester == (season + year))
                .Where(p => p.assignmentdata.courses.Subjectabbreviation == subject)
                .Where(p => p.assignmentdata.courses.Coursenumber == num)
                .Where(p => p.assignmentdata.classinfo.combo.assignments.Categorynames == category)
                .Where(p => p.assignmentdata.classinfo.combo.assignments.Assignmentname == asgname)
                .Select(p => new
                {
                    fname = p.students.Firstname,
                    lname = p.students.Lastname,
                    uid = p.students.Userid,
                    time = p.assignmentdata.classinfo.combo.subs.Submissiontime,
                    score = p.assignmentdata.classinfo.combo.subs.Score,
                }).ToArray();
            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {

            var classes = db.Classes
                .Join(db.Courses,
                    classes => classes.Catalogid,
                    courses => courses.Catalogid,
                    (classes, courses) => new { classes, courses })
                .Join(db.Users,
                    combo => combo.classes.Professorid,
                    professors => professors.Userid,
                    (combo, professors) => new
                        { combo, professors })
                .Where(p => "u" + p.professors.Userid == uid)
                .Select(p => new
                {
                    semester = p.combo.classes.Semester,
                    sub = p.combo.courses.Subjectabbreviation,
                    num = p.combo.courses.Coursenumber,
                    nm = p.combo.courses.Coursename
                })
                .AsEnumerable()
                .Select(p => new
                {
                    subject = p.sub,
                    number = p.num,
                    name = p.nm,
                    season = p.semester?.Split(' ').FirstOrDefault() ?? "",
                    year = p.semester?.Split(' ').Skip(1).FirstOrDefault() ?? ""
                });
            return Json(classes);
        }


        
        /*******End code to modify********/
    }
}

