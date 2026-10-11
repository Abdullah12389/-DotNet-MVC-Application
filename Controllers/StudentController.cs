using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

public class StudentController : Controller
{
    private readonly StudentRepository _repository;
    public StudentController(StudentRepository repository)
    {
        _repository = repository;
    }
    public async Task<IActionResult> Index()
    {
        List<Student> students= await _repository.GetAllRegisteredAsync();
        return View(students);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(Student student)
    {
        await _repository.AddNewStudent(student.StudentName,student.RegistrationNumber,student.Department,student.AttendancePercentage);
        return RedirectToAction("Index");
    } 
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _repository.GetStudentById(id);
        if (student == null)
        {
            return NotFound();
        }
        return View(student);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(Student student)
    {
        bool success = await _repository.EditStudent(student.StudentId,student.StudentName,student.RegistrationNumber,student.Department,student.AttendancePercentage);
        if (success)
        {
            return RedirectToAction("Index");
        }
        return NotFound();
    } 
        [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        bool success = await _repository.DeleteStudent(id);
        if (!success)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }
}