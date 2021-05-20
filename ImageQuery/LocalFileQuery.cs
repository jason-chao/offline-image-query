using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

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

        public static ICollection<ImageLabel> LoadLabelData(string dataFilename, string delimiter = ",")
        {
            var dataRows = File.ReadAllLines(dataFilename).Distinct();
            var fileLabelPairs = dataRows.Select(r => r.Split(delimiter));

            List<ImageLabel> imageLabels = new List<ImageLabel>();

            foreach (var pair in fileLabelPairs)
            {
                string labels = pair[1];
                string imageFilename = pair[0].Trim();

                foreach (var label in labels.Split(";").Select(l => l.Trim()))
                {
                    imageLabels.Add(new ImageLabel() { Label = label, Filename = imageFilename });
                }
            }

            return imageLabels;
        }
    }
}
