using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For Key attribute

namespace DCPWorkCenter_Api.Models;

public partial class SourseAssign
{
    [Key] // Defines CourseAssignId as primary key
    public string CourseAssignId { get; set; } = null!; // null! means it is required

    public string? Uid { get; set; }

    public string? SubjectCode { get; set; }

    public string? EntryByUid { get; set; }

    public DateTime? LastUpdate { get; set; }

    // Use bool if this is bit or boolean in database;
    // otherwise, if string, your previous code is fine
    public string? Activate { get; set; }

    // Newly added property to carry video location in the API
    public string? VideoPath { get; set; }
}