using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageQuery
{
    public class ImageLabel
    {
        public string Label { get; set; }
        public string Filename { get; set; }

        public FileInfo FileInfo { get => new FileInfo(Filename); }
    }
}
