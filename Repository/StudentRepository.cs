using System.Data;
using Microsoft.Data.SqlClient;

public class StudentRepository
{
    private readonly string _connectionString;
    public StudentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<List<Student>> GetAllRegisteredAsync()
    {
        List<Student> students = new List<Student>{};
        string query = "Select * from students";
        DataTable studentTable = new DataTable();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using SqlCommand getStudentsCommand = new SqlCommand(query, connection);
            using SqlDataAdapter studentAdapter = new SqlDataAdapter(getStudentsCommand);
            await Task.Run(()=>studentAdapter.Fill(studentTable));
        }
        foreach(DataRow stdRow in studentTable.Rows)
        {
            Student std = new Student
            {
                StudentId = Convert.ToInt32(stdRow["StudentId"]),
                StudentName = stdRow["StudentName"].ToString() ?? "",
                RegistrationNumber = stdRow["RegistrationNumber"].ToString() ?? "",
                Department = stdRow["Department"].ToString() ?? "",
                AttendancePercentage = Convert.ToDouble(stdRow["AttendancePercentage"])
            };
            students.Add(std);                
        }
        return students;
    }
    public async Task<Student?> GetStudentById(int id)
    {
        Student? student = null;
        string query = "Select * from students where studentid = @id";
        DataTable studentTable = new DataTable();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using SqlCommand getStudentsCommand = new SqlCommand(query, connection);
            getStudentsCommand.Parameters.AddWithValue("@id",id);
            using SqlDataAdapter studentAdapter = new SqlDataAdapter(getStudentsCommand);
            await Task.Run(()=>studentAdapter.Fill(studentTable));
        }
        DataRow std = studentTable.Rows[0];
        {
            student = new Student
            {
                StudentId = Convert.ToInt32(std["StudentId"]),
                StudentName = std["StudentName"].ToString() ?? "",
                RegistrationNumber = std["RegistrationNumber"].ToString() ?? "",
                Department = std["Department"].ToString() ?? "",
                AttendancePercentage = Convert.ToDouble(std["AttendancePercentage"])
            };               
        }
        return student;
    }
    public async Task<bool> AddNewStudent(string name,string regno,string dep,double att)
    {
        string query = "Select * from students where 1=0";
        DataTable studentTable = new DataTable();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using SqlCommand getStudentsCommand = new SqlCommand(query, connection);
            using SqlDataAdapter studentAdapter = new SqlDataAdapter(getStudentsCommand);
            await Task.Run(()=>studentAdapter.FillSchema(studentTable,SchemaType.Source));
            using SqlCommand addStudent = new SqlCommand(
                "Insert into students (StudentName,RegistrationNumber,Department,AttendancePercentage) values (@name,@reg,@dep,@attPer)",
                connection
            );
            addStudent.Parameters.Add("@name",SqlDbType.VarChar,100,"StudentName");
            addStudent.Parameters.Add("@reg", SqlDbType.VarChar,30,"RegistrationNumber");
            addStudent.Parameters.Add("@dep", SqlDbType.VarChar,50,"Department");
            SqlParameter attendancePercent = new SqlParameter("@attPer", SqlDbType.Decimal) 
            { 
                Precision = 5, 
                Scale = 2, 
                SourceColumn = "AttendancePercentage" 
            };
            addStudent.Parameters.Add(attendancePercent);
            studentAdapter.InsertCommand = addStudent;
            DataRow newStudent = studentTable.NewRow();
            newStudent["StudentName"] = name;
            newStudent["RegistrationNumber"] = regno;
            newStudent["Department"] = dep;
            newStudent["AttendancePercentage"] = Convert.ToDecimal(att);
            studentTable.Rows.Add(newStudent);
            int rows = await Task.Run(()=>studentAdapter.Update(studentTable));
            if(rows>=1){
                return true;
            }
        }
        return false;
    }
    public async Task<bool> EditStudent(int id,string? name,string? regno,string? dep,double? att)
    {
        string query = "Select * from students where StudentId=@id";
        DataTable studentTable = new DataTable();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using SqlCommand getStudentsCommand = new SqlCommand(query, connection);
            getStudentsCommand.Parameters.AddWithValue("@id",id);
            using SqlDataAdapter studentAdapter = new SqlDataAdapter(getStudentsCommand);
            studentAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            await Task.Run(()=>studentAdapter.Fill(studentTable));
            using SqlCommand updateStudent = new SqlCommand(
                "Update students set StudentName=@name,RegistrationNumber=@reg,Department=@dep,AttendancePercentage=@AttPer where StudentId=@id",
                connection
            );
            updateStudent.Parameters.Add("@id", SqlDbType.Int,4,"StudentId");
            updateStudent.Parameters.Add("@name",SqlDbType.VarChar,100,"StudentName");
            updateStudent.Parameters.Add("@reg", SqlDbType.VarChar,30,"RegistrationNumber");
            updateStudent.Parameters.Add("@dep", SqlDbType.VarChar,50,"Department");
            SqlParameter attendancePercent = new SqlParameter("@AttPer", SqlDbType.Decimal) 
            { 
                Precision = 5, 
                Scale = 2, 
                SourceColumn = "AttendancePercentage" 
            };
            updateStudent.Parameters.Add(attendancePercent);
            studentAdapter.UpdateCommand = updateStudent;
            DataRow existingStudent = studentTable.Rows[0];
            if (existingStudent != null)
            {
                if(name!=null)  existingStudent["StudentName"] = name;
                if(regno!=null) existingStudent["RegistrationNumber"] = regno;
                if(dep!=null)   existingStudent["Department"] = dep;
                if(att!=null)   existingStudent["AttendancePercentage"] = Convert.ToDecimal(att);
            }
            int rows = await Task.Run(()=>studentAdapter.Update(studentTable));
            if(rows>=1){
                return true;
            }   
        }
        return false;
    }
    public async Task<bool> DeleteStudent(int id)
    {
        string query = "Select * from students where StudentId=@id";
        DataTable studentTable = new DataTable();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using SqlCommand getStudentsCommand = new SqlCommand(query, connection);
            getStudentsCommand.Parameters.AddWithValue("@id",id);
            using SqlDataAdapter studentAdapter = new SqlDataAdapter(getStudentsCommand);
            studentAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            await Task.Run(()=>studentAdapter.Fill(studentTable));
            if (studentTable.Rows.Count == 0)
            {
                return false; // Student ID was not found
            }
            using SqlCommand deleteStudent = new SqlCommand(
                "Delete From students where StudentId=@id",
                connection
            );
            deleteStudent.Parameters.Add("@id", SqlDbType.Int,4,"StudentId");
            studentAdapter.DeleteCommand = deleteStudent;
            DataRow? toDelStd = studentTable.Rows[0];
            if (toDelStd != null)
            {
                toDelStd.Delete();
                int rows = await Task.Run(()=>studentAdapter.Update(studentTable));
                if(rows>=1){
                    return true;
                }   
            }
        }
        return false;
    }
}