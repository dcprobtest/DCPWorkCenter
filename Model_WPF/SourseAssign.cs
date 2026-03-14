using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Key এট্রিবিউট এর জন্য

namespace DCPWorkCenter_Api.Models;

public partial class SourseAssign
{
    [Key] // এটি কোর্স এসাইন আইডিকে প্রাইমারি কি হিসেবে ডিফাইন করবে
    public string CourseAssignId { get; set; } = null!; // null! মানে এটি রিকোয়ার্ড

    public string? Uid { get; set; }

    public string? SubjectCode { get; set; }

    public string? EntryByUid { get; set; }

    public DateTime? LastUpdate { get; set; }

    // ডাটাবেসে যদি এটি bit বা boolean হয় তবে bool ব্যবহার করুন
    // অন্যথায় string হলে আপনার আগের কোডই ঠিক আছে
    public string? Activate { get; set; }

    // নতুন যোগ করা প্রপার্টি যা ভিডিওর লোকেশন API-তে বহন করবে
    public string? VideoPath { get; set; }
}