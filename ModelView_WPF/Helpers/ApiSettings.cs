using System;
using System.Configuration;

namespace ModelView_WPF.Helpers
{
    public static class ApiSettings
    {
        public static string BaseUrl =>
            (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://74.208.227.170:8050").TrimEnd('/');

        public static string SaveUserDocUrl() => $"{BaseUrl}/api/Data/SaveUserDoc";
        public static string DownloadFileByUidAndTypeUrl(string uid, string docType) => $"{BaseUrl}/api/Data/DownloadFileByUidAndType/{uid}/{docType}";
        public static string GetEmployeeDetailUrl() => $"{BaseUrl}/api/Data/GetEmployeeDetail/";
        public static string GetSubjectCodeUrl() => $"{BaseUrl}/api/Data/GetSubjectCode";

        public static string AccEmpSalaryGetExpSubCodesUrl => $"{BaseUrl}/api/AccEmpSalary/GetExpSubCodes";
        public static string AccEmpSalaryBaseUrl => $"{BaseUrl}/api/AccEmpSalary";

        public static string GetAccEmpSalaryByIdUrl(int id) => $"{BaseUrl}/api/AccEmpSalary/{id}";
        public static string UpdateAccEmpSalaryUrl(int id) => $"{BaseUrl}/api/AccEmpSalary/{id}";

        public static string AccExpBubBaseUrl => $"{BaseUrl}/api/AccExpBub";
        public static string AccExpBubGetExpendHeadCodesUrl => $"{BaseUrl}/api/AccExpBub/GetExpendHeadCodes";
        public static string AccExpendHeadBaseUrl => $"{BaseUrl}/api/AccExpendHead";
        public static string AccIncomeHeadBaseUrl => $"{BaseUrl}/api/AccIncomeHead";
        public static string GetAccIncomeHeadByIdUrl(string id) => $"{BaseUrl}/api/AccIncomeHead/{id}";
        public static string AccIncSubBaseUrl => $"{BaseUrl}/api/AccIncSub";
        public static string AccIncSubGetIncomeHeadCodesUrl => $"{BaseUrl}/api/AccIncSub/IncomeHeadCodes";
        public static string AccUSExpBubBaseUrl => $"{BaseUrl}/api/AccUSExpBub";
        public static string AccUSExpBubGetExpCodesUrl => $"{BaseUrl}/api/AccUSExpBub/GetExpCodes";
        public static string AccUSExpendHeadBaseUrl => $"{BaseUrl}/api/AccUSExpendHead";
        public static string GetAccUSExpendHeadByIdUrl(string id) => $"{BaseUrl}/api/AccUSExpendHead/{id}";
        public static string AccUSIncomeHeadBaseUrl => $"{BaseUrl}/api/AccUSIncomeHead";
        public static string GetAccUSIncomeHeadByIdUrl(string id) => $"{BaseUrl}/api/AccUSIncomeHead/{id}";
        public static string AccUSIncSubBaseUrl => $"{BaseUrl}/api/AccUSIncSub";
        public static string GetAccUSIncSubByIdUrl(string id) => $"{BaseUrl}/api/AccUSIncSub/{id}";
        public static string GetAccUSIncSubIncCodesUrl() => $"{BaseUrl}/api/AccUSIncSub/GetIncCodes";
        public static string AssignmentScrumBaseUrl => $"{BaseUrl}/api/AssignmentScrum";
        public static string GetAssignmentScrumAvailableAssignIDsUrl() => $"{BaseUrl}/api/AssignmentScrum/GetAvailableAssignIDs";
        public static string GetAssignmentScrumAvailableScrumIDsUrl() => $"{BaseUrl}/api/AssignmentScrum/GetAvailableScrumIDs";
        public static string AssignmentScrumUpdateStatusUrl => $"{BaseUrl}/api/AssignmentScrum/UpdateStatus";
        public static string AssignTechInfoBaseUrl => $"{BaseUrl}/api/AssignTechInfo";
        public static string GetAssignTechInfoUpdateUrl(int id) => $"{BaseUrl}/api/AssignTechInfo/{id}";
        public static string ChatBotBaseUrl => $"{BaseUrl}/api/ChatBot";
        public static string GetChatBotHistoryUrl(string uid) => $"{BaseUrl}/api/ChatBot/{uid}";
        public static string ClassModuleBaseUrl => $"{BaseUrl}/api/ClassModule";
        public static string GetClassSectionCoursesUrl() => $"{BaseUrl}/api/Data/GetCourses";
        public static string GetClassSectionsUrl() => $"{BaseUrl}/api/Data/GetSections";
        public static string SaveClassSectionUrl() => $"{BaseUrl}/api/Data/SaveSection";
        public static string DeleteClassSectionUrl(string sectionId) => $"{BaseUrl}/api/Data/DeleteSection/{sectionId}";
        public static string GetClassSubjectsUrl() => $"{BaseUrl}/api/Data/GetSubjects";
        public static string SaveClassSubjectUrl() => $"{BaseUrl}/api/Data/SaveSubject";
        public static string DeleteClassSubjectUrl(string subjectCode) => $"{BaseUrl}/api/Data/DeleteSubject/{subjectCode}";
        public static string GetClassVideosUrl() => $"{BaseUrl}/api/Data/GetVideos";
        public static string SaveClassVideoUrl() => $"{BaseUrl}/api/Data/SaveVideo";
        public static string DeleteClassVideoUrl(string classId) => $"{BaseUrl}/api/Data/DeleteVideo/{classId}";
        public static string GetClassWiseExamBatchStudentsUrl() => $"{BaseUrl}/api/ClassWiseExamBatchStudent/GetStudents";
        public static string SaveClassWiseExamBatchStudentUrl() => $"{BaseUrl}/api/ClassWiseExamBatchStudent";
        public static string DeleteClassWiseExamBatchStudentUrl(string studentId) => $"{BaseUrl}/api/ClassWiseExamBatchStudent/{studentId}";
        public static string GetClassWiseExamBatchesUrl() => $"{BaseUrl}/api/ClassWiseExamBatch";
        public static string GetClassWiseExamSectionsUrl() => $"{BaseUrl}/api/ClassWiseExam/Sections";
        public static string GetClassWiseExamsUrl() => $"{BaseUrl}/api/ClassWiseExam";
        public static string SaveClassWiseExamUrl() => $"{BaseUrl}/api/ClassWiseExam";
        public static string SaveDepartmentUrl() => $"{BaseUrl}/api/Data/SaveDepartment";
        public static string SaveDesignationUrl() => $"{BaseUrl}/api/Data/SaveDesignation";
        public static string SaveEmployeeUrl() => $"{BaseUrl}/api/Data/SaveEmployee";
        public static string GetScrumInfoRoute() => $"{BaseUrl}/api/ScrumInfo";
        public static string ScrumHubUrl() => $"{BaseUrl}/scrumHub";
        public static string GetLastTransIDUrl() => $"{BaseUrl}/api/Data/GetLastTransID";
        public static string SaveSTEMUrl() => $"{BaseUrl}/api/Data/SaveSTEM";
        public static string GetSubjectsUrl() => $"{BaseUrl}/api/Data/GetSubjects";
        public static string GetSectionsUrl() => $"{BaseUrl}/api/Data/GetSections";
        public static string GetGroupUsersUrl() => $"{BaseUrl}/api/Data/GetGroupUsers";
        public static string GetVideosUrl() => $"{BaseUrl}/api/Data/GetVideos";
        public static string SaveSubjectWiseAssignUrl() => $"{BaseUrl}/api/Data/SaveSubjectWiseAssign";
        public static string GetTechInfoUrl() => $"{BaseUrl}/api/TechInfo";
        public static string GetUserNameUrl(string uid) => $"{BaseUrl}/api/Data/GetUserName/{uid}";
        public static string GetEmployeeInfoUrl() => $"{BaseUrl}/api/Data/GetEmployeeInfo";
        public static string CreateUserDocRequestUrl() => $"{BaseUrl}/api/Data/CreateUserDocRequest";
        public static string SaveUserUrl() => $"{BaseUrl}/api/Data/SaveUser";
        public static string GetUsersUrl => $"{BaseUrl}/api/Data/GetUsers";
        public static string LoginUrl() => $"{BaseUrl}/api/Data/Login";

        public static string LoginSessionInfoUrl() => $"{BaseUrl}/api/LoginSessionInfo";
        public static string GetDepartmentsUrl => $"{BaseUrl}/api/Data/GetDepartments";
        public static string GetDesignationsUrl => $"{BaseUrl}/api/Data/GetDesignations";
        public static string ClassFocusMonitorUrl() => $"{BaseUrl}/api/ClassFocusMonitor";
        public static string ClassFocusMonitorBaseUrl => $"{BaseUrl}/api/ClassFocusMonitor";
        public static string ClassFocusMonitorUIDsUrl => $"{BaseUrl}/api/ClassFocusMonitor/GetUIDs";
        public static string GetClassFocusByUidUrl(string uid) => $"{BaseUrl}/api/ClassFocusMonitor/GetByUID?uid={uid}";
        public static string UploadActivityScreenshotUrl() => $"{BaseUrl}/api/UserActivity/UploadActivityScreenshot";
        public static string GetActivityUIDsUrl => $"{BaseUrl}/api/UserActivity/GetActivityUIDs";
        public static string GetScreenshotsUrl(string uid, string dateStr) => $"{BaseUrl}/api/UserActivity/GetScreenshots/{uid}/{dateStr}";
        public static string GetScreenshotImageUrl(string uid, string fileName) => $"{BaseUrl}/api/UserActivity/GetScreenshotImage/{uid}/{fileName}";
        public static string UpdateClassFocusUrl(int sessionId) => $"{BaseUrl}/api/ClassFocusMonitor/{sessionId}";
        public static string DeleteClassFocusUrl(int sessionId) => $"{BaseUrl}/api/ClassFocusMonitor/{sessionId}";
        public static string UserDropdownUrl => $"{BaseUrl}/api/AssignmentScrum/GetAvailableUsers";

        public static string GetAssignedVideosUrl(string uid) => $"{BaseUrl}/api/Data/GetAssignedVideos/{uid}";
    }
}