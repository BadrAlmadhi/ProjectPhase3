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
                .Join(db.Users,
                    enrollments => enrollments.Userid,
                    users => users.Userid,
                    (enrollments, users) => new { enrollments, users })
                .Join(db.Classes,
                    combo => combo.enrollments.Classid,
                    classes => classes.Classid,
                    (combo, classes) => new { combo, classes })
                .Join(db.Courses,
                    classinfo => classinfo.classes.Catalogid,
                    courses => courses.Catalogid,
                    (classinfo, courses) => new { classinfo, courses })
                .Where(p => p.courses.Coursenumber == num)
                .Where(p => p.courses.Subjectabbreviation == subject)
                .Where(p => p.classinfo.classes.Semester != null && p.classinfo.classes.Semester.StartsWith(season))
                .Where(p => p.classinfo.classes.Semester != null && p.classinfo.classes.Semester.EndsWith(year.ToString()))
                .Select(p => new
                {
                    fname = p.classinfo.combo.users.Firstname,
                    lname = p.classinfo.combo.users.Lastname,
                    uid = p.classinfo.combo.users.Userid,
                    dob = p.classinfo.combo.users.Dob,
                    grade = p.classinfo.combo.enrollments.Grade
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
            string semester = season + " " + year;
    
            var assignments = db.Assignments
                .Join(db.Classes,
                    a => a.Classid,
                    c => c.Classid,
                    (a, c) => new { assignment = a, classid = c.Classid, classobj = c })
                .Join(db.Courses,
                    ac => ac.classobj.Catalogid,
                    co => co.Catalogid,
                    (ac, co) => new { ac.assignment, ac.classid, ac.classobj, course = co })
                .Where(p => p.course.Coursenumber == num)
                .Where(p => p.course.Subjectabbreviation == subject)
                .Where(p => p.classobj.Semester == semester)
                .Where(p => p.assignment.Assignmentname != null)
                .Where(p => category == null || p.assignment.Categorynames == category)
                .Select(p => new
                {
                    aname = p.assignment.Assignmentname,
                    cname = p.assignment.Categorynames,
                    due = p.assignment.Duedate,
                    submissions = db.Submissions.Count(s => s.Assignmentid == p.assignment.Assignmentid && s.Classid == p.classid)
                })
                .ToArray();
    
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
            string semester = season + " " + year;
    
            var assignmentcats = db.Assignmentcategories
                .Join(db.Classes,
                    ac => ac.Classid,
                    c => c.Classid,
                    (ac, c) => new { category = ac, classobj = c })
                .Join(db.Courses,
                    combo => combo.classobj.Catalogid,
                    co => co.Catalogid,
                    (combo, co) => new { combo, course = co })
                .Where(p => p.course.Coursenumber == num)
                .Where(p => p.course.Subjectabbreviation == subject)
                .Where(p => p.combo.classobj.Semester == semester)
                .Select(p => new
                {
                    name = p.combo.category.Categorynames,
                    weight = p.combo.category.Gradingweight
                })
                .ToArray();
        
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
            try
            {
                if (string.IsNullOrWhiteSpace(category) || catweight < 0 || catweight > 100)
                {
                    return Json(new { success = false, error = "Invalid category name or weight" });
                }

                string semester = season + " " + year;

                var classInfo = db.Courses
                    .Join(db.Classes,
                        courses => courses.Catalogid,
                        classes => classes.Catalogid,
                        (courses, classes) => new { courses, classes })
                    .Where(p => p.classes.Semester == semester)
                    .Where(p => p.courses.Subjectabbreviation == subject)
                    .Where(p => p.courses.Coursenumber == num)
                    .Select(p => new { classid = p.classes.Classid })
                    .FirstOrDefault();

                if (classInfo == null)
                {
                    return Json(new { success = false, error = "Class not found" });
                }

                var existingCategory = db.Assignmentcategories
                    .FirstOrDefault(ac => ac.Categorynames == category && ac.Classid == classInfo.classid);

                if (existingCategory != null)
                {
                    return Json(new { success = false, error = "Category already exists for this class" });
                }

                var newAssignmentCat = new Assignmentcategory
                {
                    Categorynames = category,
                    Gradingweight = catweight,
                    Classid = classInfo.classid
                };

                db.Assignmentcategories.Add(newAssignmentCat);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, error = "Database error: " + ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error: " + ex.Message });
            }
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
            try
            {
                var classInfo = db.Courses
                    .Join(db.Classes,
                        courses => courses.Catalogid,
                        classes => classes.Catalogid,
                        (courses, classes) => new { courses, classes })
                    .Where(p => p.classes.Semester == (season + " " + year))
                    .Where(p => p.courses.Subjectabbreviation == subject)
                    .Where(p => p.courses.Coursenumber == num)
                    .Select(p => new
                    {
                        classid = p.classes.Classid
                    })
                    .FirstOrDefault();

                if (classInfo == null)
                {
                    return Json(new { success = false });
                }

                int classid = classInfo.classid;

                // Create new assignment
                var newAssignment = new Assignment
                {
                    Categorynames = category,
                    Maxpoint = asgpoints,
                    Assignmentname = asgname,
                    Duedate = asgdue,
                    Content = asgcontents,
                    Classid = classid
                };

                db.Assignments.Add(newAssignment);
                db.SaveChanges();

                // Recalculate grades for all students in this class
                var enrolledStudents = db.Enrollments
                    .Where(e => e.Classid == classid)
                    .Select(e => e.Userid)
                    .Distinct()
                    .ToList();

                foreach (var studentid in enrolledStudents)
                {
                    var enrollment = db.Enrollments
                        .FirstOrDefault(e => e.Userid == studentid && e.Classid == classid);
                    
                    if (enrollment != null)
                    {
                        var updatedGrade = CalculateStudentClassGrade(studentid, classid);
                        enrollment.Grade = updatedGrade;
                    }
                }
                
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (DbUpdateException e)
            {
                return Json(new { success = false });
            }
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
                .Where(p => p.assignmentdata.classinfo.classes.Semester == (season + " " + year))
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


        public string GradeCalculator(double grade)
        {
            var letterGrade = "";
            if (grade >= 0.92)
            {
                letterGrade = "A";
            }
            else if (grade >= 0.90)
            {
                letterGrade = "A-";
            }
            else if (grade >= 0.87)
            {
                letterGrade = "B+";
            }
            else if (grade >= 0.82)
            {
                letterGrade = "B";
            }
            else if (grade >= 0.80)
            {
                letterGrade = "B-";
            }
            else if (grade >= 0.77)
            {
                letterGrade = "C+";
            }
            else if (grade >= 0.72)
            {
                letterGrade = "C";
            }
            else if (grade >= 0.70)
            {
                letterGrade = "C-";
            }
            else if (grade >= 0.67)
            {
                letterGrade = "D+";
            }
            else if (grade >= 0.62)
            {
                letterGrade = "D";
            }
            else if (grade >= 0.60)
            {
                letterGrade = "D-";
            }
            else // if (classid[classid.Count - 1].percentage < 0.60)
            {
                letterGrade = "E";
            }
            return letterGrade;
        }

        private string CalculateStudentClassGrade(int userid, int classid)
        {
            // Get all submissions for this student in this class
            var submissions = db.Submissions
                .Where(sub => sub.Userid == userid && sub.Classid == classid)
                .Join(db.Assignments,
                    sub => sub.Assignmentid,
                    asg => asg.Assignmentid,
                    (sub, asg) => new { sub, asg })
                .Where(p => p.asg.Classid == classid) // Filter by class after join
                .Where(p => p.sub.Score.HasValue && p.asg.Maxpoint.HasValue)
                .Select(p => new
                {
                    score = (double)p.sub.Score.Value,
                    maxpoints = (double)p.asg.Maxpoint.Value,
                    category = p.asg.Categorynames
                })
                .ToList();

            if (!submissions.Any())
            {
                return "N/A"; // No submissions yet
            }

            // Group by category and calculate average for each
            var categoryAverages = submissions
                .GroupBy(s => s.category)
                .Select(g => new
                {
                    category = g.Key,
                    average = g.Average(s => s.score / s.maxpoints)
                })
                .ToDictionary(x => x.category, x => x.average);

            // Get category weights from AssignmentCategories table for this class
            var categoryWeights = db.Assignmentcategories
                .Where(ac => ac.Classid == classid)
                .ToDictionary(ac => ac.Categorynames, ac => (ac.Gradingweight ?? 0) / 100.0);

            // Calculate weighted average
            double weightedAverage = 0;
            double totalWeight = 0;

            foreach (var category in categoryAverages)
            {
                if (categoryWeights.TryGetValue(category.Key, out var weight))
                {
                    weightedAverage += category.Value * weight;
                    totalWeight += weight;
                }
            }

            // Normalize if not all categories are present
            if (totalWeight > 0)
            {
                weightedAverage /= totalWeight;
            }
            else
            {
                return "N/A"; // No valid categories with weights
            }

            // Convert to letter grade
            return GradeCalculator(weightedAverage);
        }

        // Normalize if not all categories
        
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
            try
            {
                var assignmentInfo = db.Assignments
                    .Join(db.Classes,
                        a => a.Classid,
                        c => c.Classid,
                        (a, c) => new { assignment = a, classid = c.Classid, classobj = c })
                    .Join(db.Courses,
                        ac => ac.classobj.Catalogid,
                        co => co.Catalogid,
                        (ac, co) => new { ac.assignment, ac.classid, ac.classobj, course = co })
                    .Where(p => p.course.Subjectabbreviation == subject)
                    .Where(p => p.course.Coursenumber == num)
                    .Where(p => p.classobj.Semester == (season + year))  // ADD THIS
                    .Where(p => p.assignment.Assignmentname == asgname)
                    .Where(p => p.assignment.Categorynames == category)
                    .Select(p => new
                    {
                        assignmentid = p.assignment.Assignmentid,
                        classid = p.classid,
                        maxpoint = p.assignment.Maxpoint
                    })
                    .FirstOrDefault();

                if (assignmentInfo == null || assignmentInfo.maxpoint == null)
                {
                    return Json(new { success = false });
                }

                int userid = int.Parse(uid);
                
                // Create and save submission
                var newSub = new Submission
                {
                    Assignmentid = assignmentInfo.assignmentid,
                    Userid = userid,
                    Score = score,
                    Classid = assignmentInfo.classid,
                    Submissiontime = DateTime.Now
                };
                
                db.Submissions.Add(newSub);
                db.SaveChanges();

                // Recalculate student's overall grade
                var enrollmentRecord = db.Enrollments
                    .FirstOrDefault(e => e.Userid == userid && e.Classid == assignmentInfo.classid);

                if (enrollmentRecord != null)
                {
                    var overallGrade = CalculateStudentClassGrade(userid, assignmentInfo.classid);
                    enrollmentRecord.Grade = overallGrade;
                    db.SaveChanges();
                }
            
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
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

