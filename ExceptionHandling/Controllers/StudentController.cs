using ExceptionHandling.Exceptions;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExceptionHandling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController(ILogger<StudentController> logger) : ControllerBase
    {
        private static ICollection<Student> students = new List<Student>()
        {
            new Student{Id=1,Name="Jone",SurName="Smith"},
            new Student{Id=2,Name="Bob",SurName="German"},
            new Student{Id=3,Name="Bill",SurName="Snow"},
            new Student{Id=4,Name="Karen",SurName="Harutyunnyan"},
            new Student{Id=5,Name="Veronika",SurName="Lad"},
        };
        [HttpGet]
        public IEnumerable<Student> GetStudentsAsync()
        {
            return students;
        }
        [HttpGet("{id}")]
        public Student GetStudentById(int id)
        {
            var student = students.SingleOrDefault(s => s.Id == id);
            logger.LogInformation("Student with id {StudentId} was created",student.Id);
            if (student == null)
            {
                throw new NotFoundException(
                $"Student with id {id} was not found.");
            }
            return student;
        }
        [HttpPost]
        public Student CreateStudent(Student student)
        {
            if (student == null)
            {
                throw new BadRequestException("Student data is required.");
            }
            if (students.Any(s => s.Id == student.Id))
            {
                throw new ConflictException(
                    $"Student with id {student.Id} already exists.");
            }
            students.Add(student);
            logger.LogInformation("Student with id {StudentId} was created",student.Id);
            return student;
        }
        [HttpDelete("{id}")]
        [Authorize()]
        public Student DeleteStudent(int id)
        {
            var student = students.SingleOrDefault(s=>s.Id==id);
            if (student == null)
            {
                throw new NotFoundException(
                    $"Student with id {id} was not found.");
            }
            students.Remove(student);
            return student;
        }
        [HttpPut("{id}")]
        public Student UpdateStudent(int id, Student student)
        {
            var existingStudent = students
                .SingleOrDefault(s => s.Id == id);

            if (existingStudent == null)
            {
                throw new NotFoundException(
                    $"Student with id {id} was not found.");
            }
            if (student == null)
            {
                throw new BadRequestException(
                    "Student data is required.");
            }
            existingStudent.Name = student.Name;
            existingStudent.SurName = student.SurName;
            return existingStudent;
        }
    }
}
