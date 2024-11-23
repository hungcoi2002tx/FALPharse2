using CompareFaceExamDemo.ExternalService.Recognition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFaceExamDemo
{
    public partial class ImageCaptureForm : Form
    {
        private CompareFaceAdapterRecognitionService _compareFaceService;
        public ImageCaptureForm(CompareFaceAdapterRecognitionService compareFaceService)
        {
            InitializeComponent();
            _compareFaceService = compareFaceService;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ImageCaptureForm_Load(object sender, EventArgs e)
        {

        }

        private void btnTestClick(object sender, EventArgs e)
        {
            _compareFaceService.TestAsync();
        }
    }
}
