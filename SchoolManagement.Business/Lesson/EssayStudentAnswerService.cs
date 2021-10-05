
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;
using SchoolManagement.Business.Interfaces.LessonData;
using SchoolManagement.Data.Data;
using SchoolManagement.Master.Data.Data;
using SchoolManagement.Model;
using SchoolManagement.ViewModel;
using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Lesson;
using SchoolManagement.ViewModel.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Business
{
    public class EssayStudentAnswerService : IEssayStudentAnswerService
    {
        private readonly MasterDbContext masterDb;
        private readonly SchoolManagementContext schoolDb;
        private readonly IConfiguration config;
        private readonly ICurrentUserService currentUserService;

        public EssayStudentAnswerService(MasterDbContext masterDb, SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)
        {
            this.masterDb = masterDb;
            this.schoolDb = schoolDb;
            this.config = config;
            this.currentUserService = currentUserService;
        }



        public List<EssayStudentAnswerViewModel> GetAllEssayStudentAnswers()
        {
            var response = new List<EssayStudentAnswerViewModel>();

            var query = schoolDb.EssayStudentAnswers.Where(u => u.StudentId != null);

            var EssayStudentAnswerList = query.ToList();

            foreach (var item in EssayStudentAnswerList)
            {
                var vm = new EssayStudentAnswerViewModel
                {

                    QuestionId = item.QuestionId,
                    QuestionName = item.Question.QuestionText,
                    StudentId = item.StudentId,
                    StudentName = item.Student.FullName,
                    EssayQuestionAnswerId = item.EssayQuestionAnswerId,
                    EssayQuestionAnswerName = item.EssayQuestionAnswer.AnswerText,
                    AnswerText = item.AnswerText,
                    TeacherComments = item.TeacherComments,
                    Marks = item.Marks
                };

                response.Add(vm);
            }

            return response;
        }

        public List<DropDownViewModel> GetAllQuestions()
        {
            var questions = schoolDb.Questions
            .Where(x => x.IsActive == true)
            .Select(qe => new DropDownViewModel() { Id = qe.Id, Name = string.Format("{0}", qe.QuestionText) })
            .Distinct().ToList();

            return questions;
        }

        public List<DropDownViewModel> GetAllStudents()
        {
            var students = schoolDb.Students
            .Where(x => x.IsActive == true)
            .Select(st => new DropDownViewModel() { Id = st.Id, Name = string.Format("{0}", st.User.FullName) })
            .Distinct().ToList();

            return students;
        }

        public List<DropDownViewModel> GetAllEssayQuestionAnswers()
        {
            var essayanswers = schoolDb.EssayQuestionAnswers
            .Where(x => x.Question.Id != null)
            .Select(eq => new DropDownViewModel() { Id = eq.Id, Name = string.Format("{0}", eq.AnswerText) })
            .Distinct().ToList();

            return essayanswers;
        }

        public async Task<ResponseViewModel> SaveEssayStudentAnswer(EssayStudentAnswerViewModel vm, string userName)
        {
            var response = new ResponseViewModel();

            try
            {
                var loggedInUser = currentUserService.GetUserByUsername(userName);

                var EssayStudentAnswers = schoolDb.EssayStudentAnswers.FirstOrDefault(x => x.QuestionId == vm.QuestionId);




                if (EssayStudentAnswers == null)

                {
                    EssayStudentAnswers = new EssayStudentAnswer()
                    {
                        QuestionId = vm.QuestionId,
                        StudentId = vm.StudentId,
                        EssayQuestionAnswerId = vm.EssayQuestionAnswerId,
                        AnswerText = vm.AnswerText,
                        TeacherComments = vm.TeacherComments,
                        Marks = vm.Marks
                    };

                    schoolDb.EssayStudentAnswers.Add(EssayStudentAnswers);

                    response.IsSuccess = true;
                    response.Message = "Essay Student  Answer is Added Successfully";

                }
                else
                {
                    EssayStudentAnswers.TeacherComments = vm.TeacherComments;
                    EssayStudentAnswers.Marks = vm.Marks;


                    schoolDb.EssayStudentAnswers.Update(EssayStudentAnswers);

                    response.IsSuccess = true;
                    response.Message = "Essay Answer is Successfully Updated.";
                }

                await schoolDb.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.ToString();
            }
            return response;
        }

        public PaginatedItemsViewModel<BasicEssayStudentAnswerViewModel> GetStudentEssayList(string searchText, int currentPage, int pageSize, int questionId, int studentId)
        {
            int totalRecordCount = 0;
            double totalPages = 0;
            int totalPageCount = 0;

            var vmu = new List<BasicEssayStudentAnswerViewModel>();

            var studentanswers = schoolDb.EssayStudentAnswers.OrderBy(u => u.QuestionId);

            if (!string.IsNullOrEmpty(searchText))
            {
                studentanswers = studentanswers.Where(x => x.Question.QuestionText.Contains(searchText)).OrderBy(u => u.QuestionId);
            }

            if (questionId > 0)
            {
                studentanswers = studentanswers.Where(x => x.QuestionId == questionId).OrderBy(u => u.QuestionId);
            }

            if (studentId > 0)
            {
                studentanswers = studentanswers.Where(x => x.StudentId == studentId).OrderBy(u => u.StudentId);
            }


            totalRecordCount = studentanswers.Count();
            totalPages = (double)totalRecordCount / pageSize;
            totalPageCount = (int)Math.Ceiling(totalPages);

            var questionList = studentanswers.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            questionList.ForEach(studentanswers =>
            {
                var vm = new BasicEssayStudentAnswerViewModel()
                {
                    QuestionId = studentanswers.QuestionId,
                    QuestionName = studentanswers.Question.QuestionText,
                    StudentId = studentanswers.StudentId,
                    StudentName = studentanswers.Student.FullName,
                    EssayQuestionAnswerId = studentanswers.EssayQuestionAnswerId,
                    EssayQuestionAnswerName = studentanswers.EssayQuestionAnswer.AnswerText,
                    AnswerText = studentanswers.AnswerText,
                    TeacherComments = studentanswers.TeacherComments,
                    Marks = studentanswers.Marks


                };
                vmu.Add(vm);
            });

            var container = new PaginatedItemsViewModel<BasicEssayStudentAnswerViewModel>(currentPage, pageSize, totalPageCount, totalRecordCount, vmu);

            return container;
        }

        public DownloadFileModel downloadStudentListReport()
        {

            var essayAnswerStudentListReport  = new EssayAnswerStudentListReport();
            byte[] abytes = essayAnswerStudentListReport.PrepareReport(GetAllEssayStudentAnswers());
            

            var response = new DownloadFileModel();

            response.FileData = abytes;
            response.FileType = "application/pdf";


            return response;
        }
    }


    public class EssayAnswerStudentListReport
    {
        #region Declaration
        int _totalColumn = 4;
        Document _document;
        iTextSharp.text.Font _fontStyle;
        iTextSharp.text.pdf.PdfPTable _pdfPTable = new PdfPTable(4);
        iTextSharp.text.pdf.PdfPCell _pdfPCell;
        MemoryStream _memoryStream = new MemoryStream();
        List<EssayStudentAnswerViewModel> _essaystudentanswers = new List<EssayStudentAnswerViewModel>();
        #endregion

        public byte[] PrepareReport(List<EssayStudentAnswerViewModel> response)
        {
            _essaystudentanswers = response;

            #region
            _document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
            _document.SetPageSize(PageSize.A4);
            _document.SetMargins(20f, 20f, 20f, 20f);
            _pdfPTable.WidthPercentage = 100;
            _pdfPTable.HorizontalAlignment = Element.ALIGN_LEFT;
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 8f, 1);

            iTextSharp.text.pdf.PdfWriter.GetInstance(_document, _memoryStream);
            _document.Open();
            _pdfPTable.SetWidths(new float[] { 80f, 150f, 100f, 100f });
            #endregion

            this.ReportHeader();
            this.ReportBody();
            _pdfPTable.HeaderRows = 4;
            _document.Add(_pdfPTable);
            _document.Close();
            return _memoryStream.ToArray();

        }

        private void ReportHeader()
        {
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 18f, 1);
            _pdfPCell = new PdfPCell(new Phrase("STUDENT MARKS REPORT", _fontStyle));
            _pdfPCell.Colspan = _totalColumn;
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.Border = 0;
            _pdfPCell.BackgroundColor = BaseColor.WHITE;
            _pdfPCell.ExtraParagraphSpace = 7;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();

            _fontStyle = FontFactory.GetFont("TimesNewRoman", 12f, 1);
            _pdfPCell = new PdfPCell(new Phrase("STUDENT ESSAY MARKS LIST", _fontStyle));
            _pdfPCell.Colspan = _totalColumn;
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.Border = 0;
            _pdfPCell.BackgroundColor = BaseColor.WHITE;
            _pdfPCell.ExtraParagraphSpace = 7;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();
        }

        private void ReportBody()
        {
            #region Table header
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 1);
            _pdfPCell = new PdfPCell(new Phrase("QUESTION ID", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);


            _pdfPCell = new PdfPCell(new Phrase("STUDENT NAME", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("MARKS", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("TEACHER COMMENTS", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();
            #endregion

            #region Table Body
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 0);
            foreach (EssayStudentAnswerViewModel vm in _essaystudentanswers)
            {
                _pdfPCell = new PdfPCell(new Phrase(vm.QuestionId.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                // _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.StudentName, _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.Marks.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                // _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.TeacherComments, _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);
                _pdfPTable.CompleteRow();
            }
            #endregion

        }


    }
}

