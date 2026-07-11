using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmClass_ClassSubject : ContentPage
    {
        // মেইন কনস্ট্রাক্টর - যেটি অ্যাপ রান হওয়ার সময় UID রিসিভ করবে
        public frmClass_ClassSubject(string uid)
        {
            InitializeComponent();

            // ভিউ মডেলের সাথে ভিউ-এর বাইন্ডিং কানেক্ট করা হচ্ছে
            BindingContext = new ClassSubjectViewModel(uid);
        }

        // ডিজাইনার বা প্যারামিটার ছাড়া কল করার জন্য ব্যাকআপ কনস্ট্রাক্টর
        public frmClass_ClassSubject()
        {
            InitializeComponent();
            BindingContext = new ClassSubjectViewModel(string.Empty);
        }
    }
}