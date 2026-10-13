using System;
using System.Collections.Generic;
using System.Linq;


// ПРАКТИЧНА РОБОТА №3. ЗАВДАННЯ №3


var universityModule = new UniversityUseCaseModule();

// --- АКТОР: АДМІНІСТРАТОР (UC_CRUD: UC_RegStudent, UC_RegCourse) ---
universityModule.Authenticate("Адміністратор");

var dept = universityModule.RegisterDepartment(1, "Кафедра програмної інженерії", "Корпус 1");
var prof = universityModule.RegisterProfessor(101, "Олександр", "Іваненко", "o.ivanenko@univ.edu", dept);

var student = universityModule.RegisterStudent(1001, "Владислава", "Романчук", 2023, "КВ123456", "+380991234567", "м. Київ");
var courseDb = universityModule.RegisterCourse(501, "Бази даних та інформаційні системи", 5);
var courseOop = universityModule.RegisterCourse(502, "Об'єктно-орієнтоване програмування", 4);

// --- АКТОР: СТУДЕНТ (UC_Enroll, UC_AcademicCard, UC_ViewCourses) ---
universityModule.Authenticate("Студент");

// Перегляд каталогу курсів (UC_ViewCourses)
universityModule.PrintCourseCatalog();

// Зарахування на курси (UC_Enroll з автоматичним розширенням <<include>> UC_CheckCredits)
universityModule.EnrollStudentToCourse(1001, 501, "A");
universityModule.EnrollStudentToCourse(1001, 502, "B");

// Перегляд навчальної картки (UC_AcademicCard)
universityModule.PrintStudentAcademicCard(1001);

// --- АКТОР: ВИКЛАДАЧ (UC_Grade, UC_DeptReport) ---
universityModule.Authenticate("Викладач");

// Внесення / редагування оцінки (UC_Grade)
universityModule.UpdateStudentGrade(1001, 502, "A");

// Перегляд кадрового звіту кафедри (UC_DeptReport)
universityModule.PrintDepartmentSummary();



// КЛАС ПРОГРАМНОГО МОДУЛЯ (Управління прецедентами)

public class UniversityUseCaseModule
{
    private readonly List<Student> _students = new();
    private readonly List<Course> _courses = new();
    private readonly List<Department> _departments = new();
    private string _currentRole = "Гість";

    // UC_Auth: Авторизація в системі <<include>>
    public void Authenticate(string role)
    {
        _currentRole = role;
        Console.WriteLine($"\n[UC_Auth] Вхід у систему виконано під роллю: {_currentRole}");
    }

    // UC_CheckCredits: Перевірка ліміту ECTS <<include>>
    private bool CheckCreditsLimit(Student student, Course course, int maxCredits = 30)
    {
        int currentCredits = student.Enrollments.Sum(e => e.Course.Credits);
        if (currentCredits + course.Credits > maxCredits)
        {
            Console.WriteLine($"[UC_CheckCredits Помилка] Перевищено ліміт кредитів ({currentCredits + course.Credits}/{maxCredits} ECTS).");
            return false;
        }
        return true;
    }

    // UC_RegStudent: Реєстрація студентів та профілів (CRUD)
    public Student RegisterStudent(int id, string fn, string ln, int year, string passport, string phone, string address)
    {
        var student = new Student(id, fn, ln, year);
        var profile = new StudentProfile(id, passport, phone, address);
        student.SetProfile(profile);
        _students.Add(student);
        Console.WriteLine($"[UC_RegStudent] Зареєстровано студента: {student.GetFullName()} (ID: {id})");
        return student;
    }

    // UC_RegCourse: Управління кафедрами та курсами (CRUD)
    public Department RegisterDepartment(int id, string name, string building)
    {
        var dept = new Department(id, name, building);
        _departments.Add(dept);
        Console.WriteLine($"[UC_RegCourse] Створено кафедру: {name}");
        return dept;
    }

    public Professor RegisterProfessor(int id, string fn, string ln, string email, Department dept)
    {
        var prof = new Professor(id, fn, ln, email);
        dept.AddProfessor(prof);
        Console.WriteLine($"[UC_RegCourse] Додано викладача: {prof.GetFullName()} -> {dept.Name}");
        return prof;
    }

    public Course RegisterCourse(int id, string title, int credits)
    {
        var course = new Course(id, title, credits);
        _courses.Add(course);
        Console.WriteLine($"[UC_RegCourse] Створено курс: \"{title}\" ({credits} ECTS)");
        return course;
    }

    // UC_ViewCourses: Перегляд каталогу курсів
    public void PrintCourseCatalog()
    {
        Console.WriteLine("\n--- [UC_ViewCourses] КАТАЛОГ НАВЧАЛЬНИХ КУРСІВ ---");
        foreach (var c in _courses)
        {
            Console.WriteLine($"  • #{c.Id} {c.Title} — {c.Credits} ECTS");
        }
    }

    // UC_Enroll: Зарахування на курс
    public bool EnrollStudentToCourse(int studentId, int courseId, string initialGrade)
    {
        var student = _students.FirstOrDefault(s => s.Id == studentId);
        var course = _courses.FirstOrDefault(c => c.Id == courseId);

        if (student == null || course == null)
        {
            Console.WriteLine("[UC_Enroll Помилка] Студента або курс не знайдено.");
            return false;
        }

        // Автоматичний виклик включеного прецеденту <<include>>
        if (!CheckCreditsLimit(student, course)) return false;

        student.EnrollInCourse(course, initialGrade);
        Console.WriteLine($"[UC_Enroll] Студента {student.GetFullName()} зараховано на курс \"{course.Title}\"");
        return true;
    }

    // UC_Grade: Внесення та редагування оцінок
    public void UpdateStudentGrade(int studentId, int courseId, string newGrade)
    {
        var student = _students.FirstOrDefault(s => s.Id == studentId);
        var enrollment = student?.Enrollments.FirstOrDefault(e => e.Course.Id == courseId);

        if (enrollment != null)
        {
            enrollment.Grade = newGrade;
            Console.WriteLine($"[UC_Grade] Оновлено оцінку студенту {student!.GetFullName()} з курсу \"{enrollment.Course.Title}\": {newGrade}");
        }
    }

    // UC_AcademicCard: Перегляд академічної картки
    public void PrintStudentAcademicCard(int studentId)
    {
        var student = _students.FirstOrDefault(s => s.Id == studentId);
        if (student == null) return;

        Console.WriteLine($"\n--------------------------------------------------");
        Console.WriteLine($" [UC_AcademicCard] АКАДЕМІЧНА КАРТКА #{student.Id}");
        Console.WriteLine($"--------------------------------------------------");
        Console.WriteLine($"ПІБ:          {student.GetFullName()}");
        Console.WriteLine($"Телефон:      {student.Profile?.PhoneNumber}");
        Console.WriteLine("Зараховані дисципліни:");

        foreach (var e in student.Enrollments)
        {
            Console.WriteLine($"  - {e.Course.Title} ({e.Course.Credits} ECTS) | Оцінка: {e.Grade}");
        }

        Console.WriteLine($"Загалом ECTS: {student.Enrollments.Sum(e => e.Course.Credits)}");
        Console.WriteLine($"--------------------------------------------------\n");
    }

    // UC_DeptReport: Перегляд кадрового звіту кафедри
    public void PrintDepartmentSummary()
    {
        Console.WriteLine("\n--------------------------------------------------");
        Console.WriteLine(" [UC_DeptReport] КАДРОВИЙ ЗВІТ ПО КАФЕДРАХ");
        Console.WriteLine("--------------------------------------------------");
        foreach (var dept in _departments)
        {
            Console.WriteLine($"Кафедра: {dept.Name} ({dept.Building})");
            foreach (var prof in dept.Professors)
            {
                Console.WriteLine($"  • Викладач: {prof.GetFullName()} [{prof.Email}]");
            }
        }
        Console.WriteLine("--------------------------------------------------\n");
    }
}



// КЛАСИ ДОМЕННОЇ МОДЕЛІ


public class Department
{
    public int Id { get; }
    public string Name { get; }
    public string Building { get; }
    private readonly List<Professor> _professors = new();
    public IReadOnlyCollection<Professor> Professors => _professors.AsReadOnly();

    public Department(int id, string name, string building) => (Id, Name, Building) = (id, name, building);

    public void AddProfessor(Professor professor) => _professors.Add(professor);
}

public class Professor
{
    public int Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public Professor(int id, string fn, string ln, string email) => (Id, FirstName, LastName, Email) = (id, fn, ln, email);
    public string GetFullName() => $"{FirstName} {LastName}";
}

public class Student
{
    public int Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public int EnrollmentYear { get; }
    public StudentProfile? Profile { get; private set; }
    private readonly List<Enrollment> _enrollments = new();
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    public Student(int id, string fn, string ln, int year) => (Id, FirstName, LastName, EnrollmentYear) = (id, fn, ln, year);

    public void SetProfile(StudentProfile profile) => Profile = profile;

    public void EnrollInCourse(Course course, string grade)
    {
        var enrollment = new Enrollment(this, course, grade);
        _enrollments.Add(enrollment);
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}

public class StudentProfile
{
    public int Id { get; }
    public string PassportNumber { get; }
    public string PhoneNumber { get; }
    public string Address { get; }

    public StudentProfile(int id, string passport, string phone, string address) =>
        (Id, PassportNumber, PhoneNumber, Address) = (id, passport, phone, address);
}

public class Course
{
    public int Id { get; }
    public string Title { get; }
    public int Credits { get; }

    public Course(int id, string title, int credits) => (Id, Title, Credits) = (id, title, credits);
}

public class Enrollment
{
    public Student Student { get; }
    public Course Course { get; }
    public string Grade { get; set; }

    public Enrollment(Student student, Course course, string grade) => (Student, Course, Grade) = (student, course, grade);
}