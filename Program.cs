using CSV.Models;
using CSV.Models.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
namespace CSV
{
    class Program
    {
        static void Main(string[] args)
        {

            List<string> directories = new List<string>();
            directories = FTP.GetDirectory(Constants.FTP.BaseUrl);

            List<Student> students_list = new List<Student>();


            foreach (var directory in directories)
            {
                Console.WriteLine("Directory: " + directory);
            }

            foreach (var directory in directories)
            {
                Student student = new Student() { AbsoluteUrl = Constants.FTP.BaseUrl };
                student.FromDirectory(directory);

                string infoFilePath = student.FullPathUrl + "/" + Constants.Locations.InfoFile;

                bool fileExists = FTP.FileExists(infoFilePath);
                if (fileExists == true)
                {

                    Console.WriteLine("Found info file:");

                    byte[] bytes = FTP.DownloadFileData(infoFilePath);

                    string csvData = Encoding.Default.GetString(bytes);

                    string[] csvlines = csvData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    if (csvlines.Length != 2)
                    {
                        Console.WriteLine("Error in CSV format.");
                    }
                    else
                    {
                        student.FromCSV(csvlines[1]);
                    }




                }
                else
                {

                    Console.WriteLine("Could not find info file:");

                }

                Console.WriteLine("Info File Path::: " + infoFilePath);

                string imageFilePath = student.FullPathUrl + "/" + Constants.Locations.ImageFile;

                bool imageFileExists = FTP.FileExists(imageFilePath);

                if (imageFileExists == true)
                {

                    Console.WriteLine("Found image file:");



                    Console.WriteLine("Image File Path::: " + imageFilePath);


                }
                else
                {

                    Console.WriteLine("Could not find image file:");


                }

                Console.WriteLine("Image File Path::: " + imageFilePath);


                students_list.Add(student);



            }



            List<JsonModel> jsons = new List<JsonModel>();

            using (StreamWriter fs = new StreamWriter(Constants.Locations.StudentCSVFile))
            {
                //int Age = 10;
                fs.WriteLine((nameof(Student.StudentId)) + ',' + (nameof(Student.FirstName)) + ',' + (nameof(Student.LastName)) + ',' + (nameof(Student.Age)) + ',' + (nameof(Student.DateOfBirth)) + ',' + (nameof(Student.MyRecord)) + ',' + (nameof(Student.ImageData)));
                foreach (var student in students_list)
                {
                    fs.WriteLine(student.ToCSV());
                    Console.WriteLine("CSV : " + student.ToCSV());
                    Console.WriteLine("String : " + student.ToString());


                    JsonModel model = new JsonModel();
                    model.Student(student);
                    jsons.Add(model);



                }
            }


            string json = System.Text.Json.JsonSerializer.Serialize(jsons);
            File.WriteAllText(Models.Constants.Locations.StudentJSONFile, json);


            string[] source = File.ReadAllLines(Constants.Locations.StudentCSVFile);
            source = source.Skip(1).ToArray();



            XElement xElement = new XElement("Root",
                from str in source
                let fields = str.Split(',')
                select new XElement("Students",
                  new XAttribute("StudentID", fields[0]),

                    new XAttribute("FirstName", fields[1]),
                    new XElement("LastName", fields[2]),
                    new XElement("Age", fields[3]),
                    new XElement("DateOfBirth", fields[4]),
                    new XElement("ImageData", fields[5])


                    )

            );
            Console.WriteLine(xElement);
            xElement.Save(Constants.Locations.StudentXMLFile);
            Console.WriteLine("Total Item in List Count: " + students_list.Count());

            int count_startswith = 0;

            foreach (var list in students_list)
            {


                if (list.FirstName.StartsWith("G"))
                {
                    count_startswith++;
                    Console.WriteLine("Starts With G: " + list);

                }
            }



            Console.WriteLine("Count Starts With G: " + count_startswith);

            //Find my record

            Student meUsingFind = students_list.Find(x => x.StudentId == "200450635");
            Console.WriteLine("My Record : " + meUsingFind);


            //Min,Average,Max age
            var average_age = students_list.Average(x => x.Age);
            var minimum_age = students_list.Min(x => x.Age);
            var maximum_age = students_list.Max(x => x.Age);



            Console.WriteLine("Average Age: " + average_age);

            Console.WriteLine("Minimum Age:" + minimum_age);
            Console.WriteLine("Maximum Age:" + maximum_age);

            CreateWordDoc.CreateWordprocessingDocument(Constants.Locations.StudentDOCFile, Constants.Locations.imgpath, students_list);
            

            FTP.UploadFile(Constants.Locations.StudentCSVFile, Constants.FTP.CSVUploadLocation);
            FTP.UploadFile(Constants.Locations.StudentXMLFile, Constants.FTP.XMLUploadLocation);
            FTP.UploadFile(Constants.Locations.StudentJSONFile, Constants.FTP.JSONUploadLocation);
            FTP.UploadFile(Constants.Locations.StudentDOCFile, Constants.FTP.DocUploadLocation);

            CreateSpreadSheet.CreateSpreadsheetWorkbook(Constants.Locations.StudentExcelFile, students_list);
            FTP.UploadFile(Constants.Locations.StudentExcelFile, Constants.FTP.ExcelUploadLocation);

            return;

        }

    }
}
       
