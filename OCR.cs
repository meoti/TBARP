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
        //internal string dataPath = "./tessdatav4/";
        //internal string language = "fra";
        //OcrEngineMode oem = OcrEngineMode.LSTM_ONLY;
        //PageSegmentationMode psm = PageSegmentationMode.AUTO_OSD;

        // bad design, now i need various OCR objects( v3, v4, from google)
        // methods
        //convert pdf to image method
        //called by getcontents 

        //public StringBuilder v4_GetContents(List<string> imgPaths)
        //{
        //    content = new StringBuilder();            
        //    try
        //    {
        //        using (var engine = new TessBaseAPI())
        //        {
        //            // Initialize tesseract-ocr 
        //            if (!engine.Init(dataPath, language, oem))
        //            {
        //                throw new Exception("Could not initialize tesseract.");
        //            }
        //            engine.SetVariable("preserve_interword_spaces", "1");
        //            engine.SetVariable("textord_dump_table_images", "0");
        //            engine.SetVariable("textord_tablefind_recognize_tables", "0");

        //            // Set the Page Segmentation mode
        //            engine.SetPageSegMode(psm);

        //            foreach (string ImagePath in imgPaths)
        //            {
        //                // Set the input image                   
        //                using (var img = engine.SetImage(ImagePath))
        //                {
        //                    // Recognize image
        //                    engine.Recognize();

        //                    ResultIterator resultIterator = engine.GetIterator();

        //                    // Extract text from result iterator
        //                   // PageIteratorLevel pageIteratorLevel = PageIteratorLevel.RIL_TEXTLINE;
        //                    using (var iter = engine.GetIterator())
        //                    {
        //                        iter.Begin();
        //                        do
        //                        {
        //                            do
        //                            {
        //                                do
        //                                {
        //                                    content.AppendLine(iter.GetUTF8Text(PageIteratorLevel.RIL_TEXTLINE));
        //                                } while (iter.Next(PageIteratorLevel.RIL_TEXTLINE));
        //                            } while (iter.Next(PageIteratorLevel.RIL_PARA));
        //                        } while (iter.Next(PageIteratorLevel.RIL_BLOCK));
        //                    }
        //                }
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceError(ex.ToString());
        //        Console.WriteLine("Unexpected Error: " + ex.Message);
        //    }
        //    return content;
        //}

        public StringBuilder v3_GetContents(List<string> imgPaths)
        {
            //with an if we can process jpg or other image formats as well without nedding to convert                 

            content = new StringBuilder();
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "fra", EngineMode.TesseractOnly))
                {
                    engine.SetVariable("preserve_interword_spaces", "1");
                    engine.SetVariable("textord_dump_table_images", "0");
                    engine.SetVariable("textord_tablefind_recognize_tables", "0");

                    // Set the Page Segmentation mode
                    //engine.SetVariable("textord_tabfind_show_columns", "1");
                    //engine.SetVariable("load_system_dawg", "0");
                    //engine.SetVariable("textord_dump_table_images", "1");
                    //engine.SetVariable("textord_tablefind_recognize_tables", "1");

                    foreach (string ImagePath in imgPaths)
                    {

                        using (var img = Pix.LoadFromFile(ImagePath))
                        {
                            using (var page = engine.Process(img, PageSegMode.Auto))
                            {
                                //var text = page.GetText();
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

