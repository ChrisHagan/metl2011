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

namespace MindTouch.Deki.Tests.PageTests
{
    [TestFixture]
    public class DeleteTests
    {
        /// <summary>
        ///     Delete a page by ID
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// </feature>
        /// <expected>200 OK HTTP Response</expected>

        [Test]
        public void DeletePageByID()
        {
            // 1. Create a page
            // 2. Delete the page by ID
            // (3) Assert page is no longer alive
            // (4) Assert page is present in archives

            Plug p = Utils.BuildPlugForAdmin();
            
            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            msg = PageUtils.DeletePageByID(p, id, true);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Page delete failed");

            msg = p.At("pages", id).GetAsync().Wait();
            Assert.AreEqual(DreamStatus.NotFound, msg.Status, "Page retrieval succeeded?!");

            msg = p.At("archive", "pages", id).Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Page archive retrieval failed");
            Assert.AreEqual(path, msg.ToDocument()["path"].AsText, "Page is not in archive?!");
        }

        /// <summary>
        ///     Delete a page by name
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// </feature>
        /// <expected>200 OK HTTP Response</expected>

        [Test]
        public void DeletePageByName()
        {
            // 1. Create a page
            // 2. Delete the page by name
            // (3) Assert page is no longer alive
            // (4) Assert page is present in archives

            Plug p = Utils.BuildPlugForAdmin();

            string id = null;
            string path = null;
            DreamMessage msg = PageUtils.CreateRandomPage(p, out id, out path);

            msg = PageUtils.DeletePageByName(p, path, true);
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Page delete failed");

            msg = p.At("pages", "=" + XUri.DoubleEncode(path)).GetAsync().Wait();
            Assert.AreEqual(DreamStatus.NotFound, msg.Status, "Page retrieval succeeded?!");

            msg = p.At("archive", "pages", id).Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status, "Page archive retrieval failed");
            Assert.AreEqual(path, msg.ToDocument()["path"].AsText, "Page is not in archive?!");
        }

        /// <summary>
        ///     Delete a leaf node (A/B/C)
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <expected>Leaf node (A/B/C) is deleted</expected>

        [Test]
        public void TestDeleteOfLeafNode()
        {
            // 1. Build a page tree
            // (2) Assert page A/B/C was created
            // 3. Delete page A/B/C
            // (4) Assert page A/B/C is no longer alive

            Plug p = Utils.BuildPlugForAdmin();

            string baseTreePath = PageUtils.BuildPageTree(p);

            DreamMessage msg = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            Assert.AreEqual("C", msg.ToDocument()["//title"].Contents);
            msg = PageUtils.DeletePageByName(p, baseTreePath + "/A/B/C", false);
            msg = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            Assert.AreEqual(DreamStatus.NotFound, msg.Status);
        }

        /// <summary>
        ///     Delete a page tree recursively
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <expected>All pages are deleted</expected>

        [Test]
        public void TestDeleteWithCascadeNoReset()
        {
            // 1. Build page tree
            // (2) Assert all pages in tree were created
            // 3. Delete A recursively
            // (4) Assert all pages in tree are no longer alive

            Plug p = Utils.BuildPlugForAdmin();

            string baseTreePath = PageUtils.BuildPageTree(p);

            Assert.AreEqual("A", PageUtils.GetPage(p, baseTreePath + "/A").ToDocument()["//title"].Contents);
            Assert.AreEqual("B", PageUtils.GetPage(p, baseTreePath + "/A/B").ToDocument()["//title"].Contents);
            Assert.AreEqual("C", PageUtils.GetPage(p, baseTreePath + "/A/B/C").ToDocument()["//title"].Contents);
            Assert.AreEqual("D", PageUtils.GetPage(p, baseTreePath + "/A/B/D").ToDocument()["//title"].Contents);
            Assert.AreEqual("E", PageUtils.GetPage(p, baseTreePath + "/A/E").ToDocument()["//title"].Contents);

            Assert.AreEqual(DreamStatus.Ok, PageUtils.DeletePageByName(p, baseTreePath + "/A", true).Status);

            Assert.AreEqual(DreamStatus.NotFound, PageUtils.GetPage(p, baseTreePath + "/A").Status);
            Assert.AreEqual(DreamStatus.NotFound, PageUtils.GetPage(p, baseTreePath + "/A/B").Status);
            Assert.AreEqual(DreamStatus.NotFound, PageUtils.GetPage(p, baseTreePath + "/A/B/C").Status);
            Assert.AreEqual(DreamStatus.NotFound, PageUtils.GetPage(p, baseTreePath + "/A/B/D").Status);
            Assert.AreEqual(DreamStatus.NotFound, PageUtils.GetPage(p, baseTreePath + "/A/E").Status);
        }

        /// <summary>
        ///     Delete root page (A) of page tree recursively as user with some pages set as private
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <assumption>A/E, A/B/D set to private</assumption>
        /// <expected>A/E, A/B/D remain unchanged; A/B/C is deleted; A and A/B are reset</expected>

        [Test]
        public void TestDeleteWithCascadeAndReset()
        {
            // 1. Build page tree
            // 2. Create user with Contributor role
            // (3) Assert all pages were created
            // 4. Restrict A/E as private (user has no delete/update access)
            // 5. Restrict A/B/D as private (user has no delete/update access)
            // 6. Delete /A* as user
            // (7) Assert A, A/B are reset; A/E, A/B/D are untouched; and A/B/C is deleted

            Plug p = Utils.BuildPlugForAdmin();

            string baseTreePath = PageUtils.BuildPageTree(p);
            string contributorUserId = null;
            string contributorUserName = null;
            UserUtils.CreateRandomContributor(p, out contributorUserId, out contributorUserName);

            DreamMessage pageAresponse = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual("A", pageAresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("B", pageABresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("C", pageABCresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("D", pageABDresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("E", pageAEresponse.ToDocument()["//title"].Contents);

            int pageA = pageAresponse.ToDocument()["@id"].AsInt.Value;
            int pageAB = pageABresponse.ToDocument()["@id"].AsInt.Value;
            int pageABC = pageABCresponse.ToDocument()["@id"].AsInt.Value;
            int pageABD = pageABDresponse.ToDocument()["@id"].AsInt.Value;
            int pageAE = pageAEresponse.ToDocument()["@id"].AsInt.Value;

            PageUtils.RestrictPage(p, baseTreePath + "/A/E", "none", "Private");
            PageUtils.RestrictPage(p, baseTreePath + "/A/B/D", "none", "Private");

            p = Utils.BuildPlugForUser(contributorUserName);
            Assert.AreEqual(DreamStatus.Ok, PageUtils.DeletePageByName(p, baseTreePath + "/A", true).Status);

            p = Utils.BuildPlugForAdmin();
            DreamMessage pageAresponse2 = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual(DreamStatus.Ok, pageAresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABDresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageAEresponse2.Status);
            Assert.AreEqual(DreamStatus.NotFound, pageABCresponse2.Status);

            int pageA2 = pageAresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAB2 = pageABresponse2.ToDocument()["@id"].AsInt.Value;
            int pageABD2 = pageABDresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAE2 = pageAEresponse2.ToDocument()["@id"].AsInt.Value;

            Assert.AreNotEqual(pageA, pageA2);
            Assert.AreNotEqual(pageAB, pageAB2);
            Assert.AreEqual(pageABD, pageABD2);
            Assert.AreEqual(pageAE, pageAE2);
        }

        /// <summary>
        ///     Delete root page (A) non-recursively as user with A/B/D set to private
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <assumption>A/B/D is private</assumption>
        /// <expected>Root page (A) reset, other pages remain unchanged</expected>

        [Test]
        public void TestDeleteWithNoCascadeAndReset()
        {
            // 1. Create page tree
            // 2. Create user with Contributor role
            // (3) Assert all pages have been created
            // 4. Restrict A/B/D as private (user has no delete/update access)
            // 5. Delete /A as user
            // (6) Assert A is reset and the rest of the pages are untouched

            Plug p = Utils.BuildPlugForAdmin();
            string baseTreePath = PageUtils.BuildPageTree(p);
            string contrUserId = null;
            string contrUserName = null;
            DreamMessage msg = UserUtils.CreateRandomContributor(p, out contrUserId, out contrUserName);

            DreamMessage pageAresponse = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual("A", pageAresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("B", pageABresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("C", pageABCresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("D", pageABDresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("E", pageAEresponse.ToDocument()["//title"].Contents);

            int pageA = pageAresponse.ToDocument()["@id"].AsInt.Value;
            int pageAB = pageABresponse.ToDocument()["@id"].AsInt.Value;
            int pageABC = pageABCresponse.ToDocument()["@id"].AsInt.Value;
            int pageABD = pageABDresponse.ToDocument()["@id"].AsInt.Value;
            int pageAE = pageAEresponse.ToDocument()["@id"].AsInt.Value;

            PageUtils.RestrictPage(p, baseTreePath + "/A/B/D", "none", "Private");

            p = Utils.BuildPlugForUser(contrUserName);
            Assert.AreEqual(DreamStatus.Ok, PageUtils.DeletePageByName(p, baseTreePath + "/A", false).Status);

            p = Utils.BuildPlugForAdmin();
            DreamMessage pageAresponse2 = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual(DreamStatus.Ok, pageAresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABDresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageAEresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABCresponse2.Status);

            int pageA2 = pageAresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAB2 = pageABresponse2.ToDocument()["@id"].AsInt.Value;
            int pageABC2 = pageABCresponse2.ToDocument()["@id"].AsInt.Value;
            int pageABD2 = pageABDresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAE2 = pageAEresponse2.ToDocument()["@id"].AsInt.Value;

            Assert.AreNotEqual(pageA, pageA2);
            Assert.AreEqual(pageAB, pageAB2);
            Assert.AreEqual(pageABC, pageABC2);
            Assert.AreEqual(pageABD, pageABD2);
            Assert.AreEqual(pageAE, pageAE2);
        }

        /// <summary>
        ///     Delete root page (A) recursively as user
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <assumption>A/B is private</assumption>
        /// <expected>Leaf pages A/B/C, A/B/D, A/E deleted; A/B is untouched; A is reset</expected>

        [Test]
        public void TestDeleteWithCascadeSkipResetNode()
        {
            // 1. Create page tree
            // 2. Create user with Contributor role
            // (3) Assert all pages have been created
            // 4. Restrict A/B as private (user has no delete/update access)
            // 5. Delete /A* as user
            // (6) Assert A is reset; A/B is untouched; and the rest of the pages are deleted

            Plug p = Utils.BuildPlugForAdmin();
            string baseTreePath = PageUtils.BuildPageTree(p);
            string contrUserId = null;
            string contrUserName = null;
            UserUtils.CreateRandomContributor(p, out contrUserId, out contrUserName);

            DreamMessage pageAresponse = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual("A", pageAresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("B", pageABresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("C", pageABCresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("D", pageABDresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("E", pageAEresponse.ToDocument()["//title"].Contents);

            int pageA = pageAresponse.ToDocument()["@id"].AsInt.Value;
            int pageAB = pageABresponse.ToDocument()["@id"].AsInt.Value;
            int pageABC = pageABCresponse.ToDocument()["@id"].AsInt.Value;
            int pageABD = pageABDresponse.ToDocument()["@id"].AsInt.Value;
            int pageAE = pageAEresponse.ToDocument()["@id"].AsInt.Value;

            PageUtils.RestrictPage(p, baseTreePath + "/A/B", "none", "Private");
            
            p = Utils.BuildPlugForUser(contrUserName);
            Assert.AreEqual(DreamStatus.Ok, PageUtils.DeletePageByName(p, baseTreePath + "/A", true).Status);

            p = Utils.BuildPlugForAdmin();
            DreamMessage pageAresponse2 = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual(DreamStatus.Ok, pageAresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABresponse2.Status);
            Assert.AreEqual(DreamStatus.NotFound, pageABDresponse2.Status);
            Assert.AreEqual(DreamStatus.NotFound, pageAEresponse2.Status);
            Assert.AreEqual(DreamStatus.NotFound, pageABCresponse2.Status);

            int pageA2 = pageAresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAB2 = pageABresponse2.ToDocument()["@id"].AsInt.Value;

            Assert.AreNotEqual(pageA, pageA2);
            Assert.AreEqual(pageAB, pageAB2);
        }

        /// <summary>
        ///     Delete A/B recursively as user
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <assumption>A/B is private</assumption>
        /// <expected>Forbidden; pages remain unchanged</expected>

        [Test]
        public void TestDeleteWithCascadeOnNoAccessNode()
        {
            // 1. Create page tree
            // 2. Create user with Contributor role
            // (3) Assert all pages have been created
            // 4. Restrict A/B as private (user has no delete/update access)
            // 5. Delete /A/B* as user
            // (6) Assert "Forbidden" response returned
            // (7) Assert no changes made

            Plug p = Utils.BuildPlugForAdmin();
            string baseTreePath = PageUtils.BuildPageTree(p);
            string contrUserId = null;
            string contrUserName = null;
            UserUtils.CreateRandomContributor(p, out contrUserId, out contrUserName);

            DreamMessage pageAresponse = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual("A", pageAresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("B", pageABresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("C", pageABCresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("D", pageABDresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("E", pageAEresponse.ToDocument()["//title"].Contents);

            int pageA = pageAresponse.ToDocument()["@id"].AsInt.Value;
            int pageAB = pageABresponse.ToDocument()["@id"].AsInt.Value;
            int pageABC = pageABCresponse.ToDocument()["@id"].AsInt.Value;
            int pageABD = pageABDresponse.ToDocument()["@id"].AsInt.Value;
            int pageAE = pageAEresponse.ToDocument()["@id"].AsInt.Value;

            PageUtils.RestrictPage(p, baseTreePath + "/A/B", "none", "Private");

            p = Utils.BuildPlugForUser(contrUserName);
            string path = "=" + XUri.DoubleEncode(baseTreePath + "/A/B");
            DreamMessage deleteResponse = p.At("pages", path).WithQuery("recursive=" + "true").DeleteAsync().Wait();
            Assert.AreEqual(DreamStatus.Forbidden, deleteResponse.Status);

            p = Utils.BuildPlugForAdmin();
            DreamMessage pageAresponse2 = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual(DreamStatus.Ok, pageAresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABDresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageAEresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABCresponse2.Status);

            int pageA2 = pageAresponse2.ToDocument()["/page/@id"].AsInt.Value;
            int pageAB2 = pageABresponse2.ToDocument()["/page/@id"].AsInt.Value;
            int pageABC2 = pageABCresponse2.ToDocument()["/page/@id"].AsInt.Value;
            int pageABD2 = pageABDresponse2.ToDocument()["/page/@id"].AsInt.Value;
            int pageAE2 = pageAEresponse2.ToDocument()["/page/@id"].AsInt.Value;

            Assert.AreEqual(pageA, pageA2);
            Assert.AreEqual(pageAB, pageAB2);
            Assert.AreEqual(pageABC, pageABC2);
            Assert.AreEqual(pageABD, pageABD2);
            Assert.AreEqual(pageAE, pageAE2);
        }

        /// <summary>
        ///     Delete page A/B non recursively
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <assumption>Tree pages: A (root), A/B, A/B/C, A/B/D, A/E</assumption>
        /// <expected>A/B is reset. Reset page becomes parent of A/B/C, A/B/D, which remain unchanged.</expected>

        [Test]
        public void TestDeleteWithNoCascadeHasChildren()
        {
            //Bug http://bugs.opengarden.org/view.php?id=1964

            // 1. Create page tree
            // (2) Assert all pages have been created
            // 3. Delete /A/B non recursive
            // (4) Assert A/B/C, A/B/D exists; A/B is reset; and _new_ A/B becomes parent of A/B/C, A/B/D

            Plug p = Utils.BuildPlugForAdmin();
            string baseTreePath = PageUtils.BuildPageTree(p);

            DreamMessage pageAresponse = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual("A", pageAresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("B", pageABresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("C", pageABCresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("D", pageABDresponse.ToDocument()["//title"].Contents);
            Assert.AreEqual("E", pageAEresponse.ToDocument()["//title"].Contents);

            int pageA = pageAresponse.ToDocument()["@id"].AsInt.Value;
            int pageAB = pageABresponse.ToDocument()["@id"].AsInt.Value;
            int pageABC = pageABCresponse.ToDocument()["@id"].AsInt.Value;
            int pageABD = pageABDresponse.ToDocument()["@id"].AsInt.Value;
            int pageAE = pageAEresponse.ToDocument()["@id"].AsInt.Value;

            DreamMessage deleteResponse = PageUtils.DeletePageByName(p, baseTreePath + "/A/B", false);
            Assert.IsTrue(deleteResponse.Status == DreamStatus.Ok);

            DreamMessage pageAresponse2 = PageUtils.GetPage(p, baseTreePath + "/A");
            DreamMessage pageABresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B");
            DreamMessage pageABCresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/C");
            DreamMessage pageABDresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/B/D");
            DreamMessage pageAEresponse2 = PageUtils.GetPage(p, baseTreePath + "/A/E");

            Assert.AreEqual(DreamStatus.Ok, pageAresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABDresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageAEresponse2.Status);
            Assert.AreEqual(DreamStatus.Ok, pageABCresponse2.Status);

            int pageA2 = pageAresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAB2 = pageABresponse2.ToDocument()["@id"].AsInt.Value;
            int pageABC2 = pageABCresponse2.ToDocument()["@id"].AsInt.Value;
            int pageABD2 = pageABDresponse2.ToDocument()["@id"].AsInt.Value;
            int pageAE2 = pageAEresponse2.ToDocument()["@id"].AsInt.Value;

            Assert.AreEqual(pageA, pageA2);
            Assert.AreNotEqual(pageAB, pageAB2);
            Assert.AreEqual(pageABC, pageABC2);
            Assert.AreEqual(pageABD, pageABD2);
            Assert.AreEqual(pageAE, pageAE2);

            Assert.AreEqual(pageAB2, pageABCresponse2.ToDocument()["page.parent/@id"].AsInt.Value);
            Assert.AreEqual(pageAB2, pageABDresponse2.ToDocument()["page.parent/@id"].AsInt.Value);
        }

        /// <summary>
        ///     Delete pages with subpages and weird titles nonrecursively 
        /// </summary>        
        /// <feature>
        /// <name>DELETE:pages/{pageid}</name>
        /// <uri>http://developer.mindtouch.com/Deki/API_Reference/DELETE%3apages%2f%2f%7bpageid%7d</uri>
        /// <parameter>recursive</parameter>
        /// </feature>
        /// <expected>Pages are reset, subpages remain unchanged</expected>

        [Test]
        public void TestForDeletePageWithSubpageAndEvilTitle()
        {
            // Bug http://bugs.opengarden.org/view.php?id=3778

            // 1. Create a page
            // 2. Create subpage to page
            // 3. Delete page nonrecursive
            // (4) Assert subpage still exists
            // 5. Repeat for several other weird page titles

            Plug p = Utils.BuildPlugForAdmin();

            //"A%4b";
            DeleteWithSubPage(p, "A%4b");
            //"A^B";
            DeleteWithSubPage(p, "A^B");
            //"Iñtërnâtiônàlizætiøn";
            DeleteWithSubPage(p, "Iñtërnâtiônàlizætiøn");
            //"A[B";
            DeleteWithSubPage(p, "A[B");
            //"GET:files/{fileid}/description";
            DeleteWithSubPage(p, "GET:files/{fileid}/description");
            //"test <div> test </div>";
            DeleteWithSubPage(p, "test <div> test </div>");
            //"<iframe src=\"http://google.com\">";
            DeleteWithSubPage(p, "<iframe src=\"http://google.com\">");
            //"Page&With Stuff?OH%5bYA\\";
            DeleteWithSubPage(p, "Page&With Stuff?OH%5bYA\\");
        }

        private void DeleteWithSubPage(Plug p, string suffix)
        {
            string path = PageUtils.GenerateUniquePageName() + suffix;
            DreamMessage msg = PageUtils.CreateRandomPage(p, path);
            string id = msg.ToDocument()["page/@id"].AsText;

            msg = PageUtils.CreateRandomPage(p, path + "/" + Utils.GenerateUniqueName());
            string subid = msg.ToDocument()["page/@id"].AsText;
            string subpath = msg.ToDocument()["page/path"].AsText;

            msg = PageUtils.DeletePageByID(p, id, false);
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            msg = p.At("pages", "=" + XUri.DoubleEncode(subpath)).GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            //Assert.IsFalse(msg.ToDocument()[string.Format("page/subpages//page/subpages/page[@id=\"{0}\"]", subid)].IsEmpty);
        }
    }
}
