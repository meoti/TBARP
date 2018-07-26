using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using System.Diagnostics;
using Tesseract;
using System.Text.RegularExpressions;

namespace InvoiceAnalyserMainUI
{
    class OCR
    {
        internal StringBuilder content;

        // methods
        //convert pdf to image method
        //called by getcontents 

        public StringBuilder GetContents(List<string> imgPaths)
        {
            //with an if we can process jpg or other image formats as well without nedding to convert                 

            content = new StringBuilder();
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "fra", EngineMode.Default))
                {
                    engine.SetVariable("preserve_interword_spaces", "1");
                    //engine.SetVariable("textord_tabfind_show_columns", "1");
                    //engine.SetVariable("load_system_dawg", "0");
                    //engine.SetVariable("textord_dump_table_images", "1");
                    //engine.SetVariable("textord_tablefind_recognize_tables", "1");

                    foreach (string ImagePath in imgPaths)
                    {

                        using (var img = Pix.LoadFromFile(ImagePath))
                        {
                            using (var page = engine.Process(img))
                            {
                                var text = page.GetText();
                                //Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                                //Console.WriteLine(text);
                                using (var iter = page.GetIterator())
                                {
                                    iter.Begin();
                                    do
                                    {
                                        do
                                        {
                                            do
                                            {
                                                content.AppendLine(iter.GetText(PageIteratorLevel.TextLine));
                                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                                    } while (iter.Next(PageIteratorLevel.Block));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Console.WriteLine("Unexpected Error: " + ex.Message);
            }
            return content;
        }



        public List<string> PDFToImage(string pdfpath)
        {
            List<string> Images = new List<string>();
            MagickReadSettings settings = new MagickReadSettings();
            settings.Density = new Density(700);

            using (MagickImageCollection images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(pdfpath, settings);

                int page = 1;
                foreach (MagickImage image in images)
                {
                    // Write page to file that contains the page number
                    image.Format = MagickFormat.Jpg;
                    image.Quality = 100;
                    //image.Threshold(new Percentage(50));
                    //image.Depth = 8;
                    image.Write(pdfpath + "_Page" + page + ".jpg");
                    Images.Add(pdfpath + "_Page" + page + ".jpg");
                    page++;
                }

            }
            return Images;
        }
    }
}

