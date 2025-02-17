/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Tests.FileTests
{
    [TestFixture]
    public class UploadTests
    {
        /// <summary>
        ///     Upload a file
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>200 OK HTTP Response</expected>

        [Test]
        public void UploadFile()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file
            string fileName = FileUtils.CreateRamdomFile(null);
            msg = DreamMessage.FromFile(fileName);
            fileName = XUri.DoubleEncode(System.IO.Path.GetFileName(fileName));
            
            // Upload file to page
            msg = p.At("pages", id, "files", "=" + fileName).Put(msg);

            // Assert file upload returned 200 OK HTTP status
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "PUT request failed");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file from Anonymous
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>401 Unauthroized HTTP Response</expected>

        [Test]
        public void UploadFileFromAnonymous()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id);

            // Acquire Anonymous permissions
            p = Utils.BuildPlugForAnonymous();

            try
            {
                // Upload a random file to page
                string fileid = null;
                string filename = null;
                msg = FileUtils.UploadRandomFile(p, id, out fileid, out filename);

                // Should not get here
                Assert.IsTrue(false, "File upload succeeded?!");
            }
            catch (DreamResponseException ex)
            {
                // Assert Unauthorized HTTP status returned
                Assert.AreEqual(DreamStatus.Unauthorized, ex.Response.Status, "Status other than \"Unauthroized\" returned");
            }

            // Acquire ADMIN permissions and delete page
            p = Utils.BuildPlugForAdmin();
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file with zero data
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>200 OK HTTP Response</expected>

        [Test]
        public void UploadFileWithZeroSize()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            /// Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with zero size and upload it to page
            byte[] content = new byte[0];
            string fileid = null;
            string filename = null;
            msg = FileUtils.UploadRandomFile(p, id, content, string.Empty, out fileid, out filename);

            // Retrieve file
            msg = p.At("files", fileid).Get();

            // Assert OK HTTP status returned
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            // Assert uploaded file content equals retrieved file content
            Assert.IsTrue(Utils.ByteArraysAreEqual(content, msg.AsBytes()));

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file on a page that has been deleted
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>404 Not Found HTTP Response</expected>

        [Test]
        public void UploadFileOnDeletedPage()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);

            try
            {
                // Attempt to upload file to deleted page
                string fileid = null;
                string filename = null;
                msg = FileUtils.UploadRandomFile(p, id, out fileid, out filename);

                // Should not get here
                Assert.IsTrue(false, "File upload succeeded?!");
            }
            catch (DreamResponseException ex)
            {
                // Assert Not Found HTTP status returned
                Assert.AreEqual(DreamStatus.NotFound, ex.Response.Status, "Status other than \"Not Found\" returned");
            }
        }

        /// <summary>
        ///     Upload a file with a long name
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>name truncated? (note: test does not verify truncation)</expected>

        [Test]
        public void UploadFileWithLongName()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with random content and with a long name
            string fileid = null;
            string filename = System.IO.Path.GetTempPath() + "file" + Utils.GenerateUniqueName();
            filename = filename.PadRight(250, 'a');
            byte[] content = FileUtils.GenerateRandomContent();
            FileUtils.CreateFile(content, filename);

            // Upload the file, assert it uploaded successfully, and store filename returned in document
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File upload failed");
            string savedFileName = msg.ToDocument()["filename"].AsText;

            // Retrieve the file and assert it retrieved successfully.
            msg = p.At("files", fileid).Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File content retrieval failed");

            // Assert retrieved file content matches generated content
            Assert.IsTrue(Utils.ByteArraysAreEqual(content, msg.AsBytes()), "Retrieved file content does not match generated content!");

            // Retrieve page files and assert file is present
            msg = p.At("pages", id, "files").Get();
            Assert.AreEqual(savedFileName, msg.ToDocument()["file/filename"].AsText, "File is not attached to page!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file with a long extension
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>name truncated? (note: test does not verify truncation)</expected>

        [Test]
        public void UploadFileWithLongExtension()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with random content and a long extension
            string fileid = null;
            string filename = System.IO.Path.GetTempPath() + "file" + Utils.GenerateUniqueName() + "." + Utils.GenerateUniqueName();
            filename = filename.PadRight(250, 'a');
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filename);

            // Upload the file, assert it uploaded successfully, and store filename returned in document
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File upload failed");
            string savedFileName = msg.ToDocument()["filename"].AsText;

            // Retrieve page files and assert file is present
            msg = p.At("pages", id, "files").Get();
            Assert.AreEqual(savedFileName, msg.ToDocument()["file/filename"].AsText, "File is not attached to page!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file with a long name... TWICE!
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>File uploaded successfully and revision count is 2</expected>

        [Test]
        public void UploadFileWithLongNameTwoTimes()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with random content and a long filename
            string fileid = null;
            string filename = System.IO.Path.GetTempPath() + "file" + Utils.GenerateUniqueName();
            filename = filename.PadRight(250, 'a');
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filename);

            // Upload the file and assert it uploaded successfully
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File upload failed!");

            // Create a file with the same name but different content and upload and replace existing file
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filename);
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);

            // Retrieve list of files attached to page
            msg = p.At("pages", id, "files").Get();

            // Assert uploaded file has 2 revisions
            Assert.AreEqual(2, msg.ToDocument()["file/revisions/@count"].AsInt, "File revision count does not equal 2!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file with a long extension... TWICE!
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>File uploaded successfully and revision count is 2</expected>

        [Test]
        public void UploadFileWithLongExtensionTwoTimes()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with random content and a long extension
            string fileid = null;
            string filename = System.IO.Path.GetTempPath() + "file" + Utils.GenerateUniqueName() + "." + Utils.GenerateUniqueName();
            filename = filename.PadRight(250, 'a');
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filename);

            // Upload the file and assert it uploaded successfully
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File upload failed!");

            // Create a file with the same name but different content and upload and replace existing file
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filename);
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);

            // Assert uploaded file has 2 revisions
            msg = p.At("pages", id, "files").Get();
            Assert.AreEqual(2, msg.ToDocument()["file/revisions/@count"].AsInt, "File revision count does not equal 2!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload a file with a config extension
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>File uploaded successfully and retrieved config file content matches generated content</expected>

        [Test]
        public void UploadFileWithConfigExtension()
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Create a file with random content and a .config extension
            byte[] content = FileUtils.GenerateRandomContent();
            string fileid = null;
            string filename = System.IO.Path.GetTempPath() + "/=file" + Utils.GenerateUniqueName() + ".config";
            FileUtils.CreateFile(content, filename);

            // Upload the file and assert it uploaded successfully
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filename);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File upload failed!");

            // Assert uploaded file has only 1 revision
            msg = p.At("pages", id, "files").Get();
            Assert.AreEqual(1, msg.ToDocument()["file/revisions/@count"].AsInt, "File revision count does not equal 1!");

            // Retrieve file, assert retrieval is successful, and assert retrieved content matches generated content
            msg = p.At("files", fileid).Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "File retrieval failed!");
            Assert.IsTrue(Utils.ByteArraysAreEqual(content, msg.AsBytes()), "Retrieved content does not match generated content!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload several files to a page
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>Page is successfully created and retrieved. Every uploaded file is present and consistent.</expected>

        [Test]
        public void PageWithManyFiles()
        {
            // Set number of files to upload and page content
            int countOfFiles = Utils.Settings.CountOfRepeats;
            StringBuilder pageContent = new StringBuilder("Page with many file links: ");

            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Upload countOfFiles number of files to page and add a link to each file in page contents
            List<string> filenames = new List<string>(countOfFiles);
            for (int i = 0; i < countOfFiles; i++)
            {
                string fileid = null;
                string filename = null;
                FileUtils.UploadRandomFile(p, id, out fileid, out filename);
                pageContent.AppendFormat("[[File:{0}/{1}|{1}]]", path, filename);
                filenames.Add(filename);
            }

            // Save page with contents
            PageUtils.SavePage(p, path, pageContent.ToString());
            
            // Retrieve the page
            msg = PageUtils.GetPage(p, path);
            
            // Assert page has exact number of uploaded files, and assert each file has been uploaded to page
            Assert.AreEqual(countOfFiles, msg.ToDocument()["files/@count"].AsInt, "Page attachment count is off");
            foreach (string filename in filenames)
                Assert.IsFalse(msg.ToDocument()[string.Format("files/file[filename=\"{0}\"]", filename)].IsEmpty, "File attachment is missing!");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Upload several images to a page
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>Page is successfully created and retrieved. Every uploaded image is present and consistent.</expected>

        [Test]
        public void PageWithManyImages()
        {
            // Set number of files to upload and page content
            int countOfFiles = Utils.Settings.CountOfRepeats;
            StringBuilder pageContent = new StringBuilder("Page with many image links: ");
            
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create random page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            // Upload countOfFiles number of images to page and add a link to each image in page contents
            for (int i = 0; i < countOfFiles; i++)
            {
                string filename = null;
                string fileid = null;
                FileUtils.UploadRandomImage(p, id, out fileid, out filename); 
                pageContent.Append(String.Format("<img src=\"File:{0}/{1}\"/>", path, filename));
            }

            // Save page with contents
            PageUtils.SavePage(p, path, pageContent.ToString());

            // Retrieve page contents in view mode and assert number of images linked is correct
            msg = p.At("pages", id, "contents").With("mode", "view").Get();
            XDoc html = XDocFactory.From("<html>" + System.Web.HttpUtility.HtmlDecode(msg.ToDocument().Contents) + "</html>", MimeType.HTML);
            Assert.AreEqual(countOfFiles, html["//img"].ListLength, "Number of images is off (view)");

            // Retrieve page contents in edit mode and assert number of images linked is correct
            msg = p.At("pages", id, "contents").With("mode", "edit").Get();
            html = XDocFactory.From("<html>" + System.Web.HttpUtility.HtmlDecode(msg.ToDocument().Contents) + "</html>", MimeType.HTML);
            Assert.AreEqual(countOfFiles, html["//img"].ListLength, "Number of images is off (edit)");

            // Retrieve page contents in raw mode and assert number of images linked is correct
            msg = p.At("pages", id, "contents").With("mode", "raw").Get();
            html = XDocFactory.From("<html>" + System.Web.HttpUtility.HtmlDecode(msg.ToDocument().Contents) + "</html>", MimeType.HTML);
            Assert.AreEqual(countOfFiles, html["//img"].ListLength, "Number of images is off (raw)");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }

        [Ignore] // http://bugs.developer.mindtouch.com/view.php?id=7759
        [Test]
        public void UploadWithNonASCIICharacters() // Document if/when fixed
        {
            //Actions:
            // Create page
            // Upload file with non ascii name
            // Try to get filename through different functions
            //Expected result: 
            // All file names must be same as original name

            Plug p = Utils.BuildPlugForAdmin();

            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            string fileid = null;
            string filename = "file" + Utils.GenerateUniqueName() + "oneåtwoäthreeöfourü.dat";
            string filepath = System.IO.Path.GetTempPath() + filename;
            FileUtils.CreateFile(FileUtils.GenerateRandomContent(), filepath);
            msg = FileUtils.UploadFile(p, id, string.Empty, out fileid, filepath);
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            msg = p.At("pages", id, "files").GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            Assert.AreEqual(filename, msg.ToDocument()[string.Format("file[@id=\"{0}\"]/filename", fileid)].AsText);

            msg = p.At("files", fileid, "info").GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            Assert.AreEqual(filename, msg.ToDocument()["filename"].AsText);

            msg = p.At("files", fileid).GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Unexpected result: {0}", msg.Status);
            Assert.AreEqual(filename, msg.ContentDisposition.FileName);

            msg = p.At("files", fileid, "=" + XUri.DoubleEncode(filename)).GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            Assert.AreEqual(filename, msg.ContentDisposition.FileName);

            //PageUtils.DeletePageByID(p, id, true);
        }

        /// <summary>
        ///     Create several revisions of a file and perform different operations on it.
        /// </summary>        
        /// <feature>
        /// <name>PUT:pages/{pageid}/files/{filename}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/PUT%3apages%2f%2f%7bpageid%7d%2f%2ffiles%2f%2f%7bfilename%7d</uri>
        /// </feature>
        /// <expected>Revision number and content remain consistent.</expected>

        [Test]
        public void MultipleRevisionContent() 
        {
            // Acquire ADMIN permissions
            Plug p = Utils.BuildPlugForAdmin();

            // Create a page
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.SavePage(p, string.Empty, PageUtils.GenerateUniquePageName(), "filerevcontenttest", out id, out path);
        
            string filename = "testfile.txt";
            string fileid = null;
            int revisionsToMake = 5;

            // Upload a file with revisionsToMake revisions
            for(int r = 1; r <= revisionsToMake; r++) {
                string content = string.Format("test file rev {0}", r);
                msg = p.At("pages", id, "files", "=" + filename).PutAsync(DreamMessage.Ok(MimeType.TEXT_UTF8, content)).Wait();
                Assert.AreEqual(DreamStatus.Ok, msg.Status, "Saving revision " + r);
                fileid = msg.ToDocument()["@id"].AsText;
                Assert.IsFalse(string.IsNullOrEmpty(fileid));
            }

            // Create a new page and move the revised file there
            msg = PageUtils.SavePage(p, string.Empty, PageUtils.GenerateUniquePageName(), "filerevcontenttest2", out id, out path);
            Assert.AreEqual(DreamStatus.Ok, p.At("files", fileid, "move").With("to", id).PostAsync().Wait().Status, "move file failed");
            
            // Create new rev
            msg = p.At("files", fileid).PutAsync(DreamMessage.Ok(MimeType.TEXT_UTF8, string.Format("test file rev {0}", revisionsToMake+2))).Wait();
            
            // Assert each revision is consistent
            for(int r = 1; r <= revisionsToMake; r++) {
                string expected = string.Format("test file rev {0}", r);
                string actual = p.At("files", fileid).With("revision", r).Get().AsText();
                Assert.AreEqual(expected, actual);
            }

            // Check contents of rev of move same as the one before
            Assert.AreEqual(string.Format("test file rev {0}", revisionsToMake), p.At("files", fileid).With("revision", (revisionsToMake + 1).ToString()).Get().AsText(), "unexpected content after move");

            // Check contents of new rev after move
            Assert.AreEqual(string.Format("test file rev {0}", revisionsToMake + 2), p.At("files", fileid).With("revision", (revisionsToMake + 2).ToString()).Get().AsText(), "unexpected content next rev after move");

            // Delete the page
            PageUtils.DeletePageByID(p, id, true);
        }   
    }
}
