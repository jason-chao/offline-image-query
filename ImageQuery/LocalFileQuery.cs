using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using CsvHelper;
using System.Globalization;

namespace ImageQuery
{
    static public class LocalFileQuery
    {

        public static ICollection<ImageLabel> FindFullFilenames(string rootDirectory, ICollection<ImageLabel> imageLabels, bool skipFilesThatAlreadyExist = true)
        {
            var dirInfo = new DirectoryInfo(rootDirectory);

            if (!dirInfo.Exists)
                return null;

            foreach (var imageLabel in imageLabels)
            {
                if (skipFilesThatAlreadyExist)
                {
                    if (imageLabel.FileInfo.Exists)
                        continue;
                }

                var files = dirInfo.GetFiles($"*{imageLabel.Filename}", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.Exists)
                    {
                        imageLabel.Filename = file.FullName;
                        continue;
                    }
                }
            }

            return imageLabels;
        }

        public static ICollection<ImageLabel> LoadLabelData(string dataFilename, string labelDelimiter = ";")
        {
            List<ImageLabel> imageLabels = new List<ImageLabel>();

            using (var reader = new StreamReader(dataFilename))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    string labels = csv.GetField(1);
                    string imageFilename = csv.GetField(0).Trim();

                    foreach (var label in labels.Split(labelDelimiter).Select(l => l.Trim()))
                    {
                        imageLabels.Add(new ImageLabel() { Label = label, Filename = imageFilename });
                    }
                }
            }

            return imageLabels;
        }
    }
}
