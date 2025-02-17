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
using System.IO;
using MindTouch.Xml;

namespace MindTouch.Deki.Import {

    public class ImportItem : IDisposable {

        //--- Fields ---
        public readonly string DataId;
        public readonly XDoc Request;
        public readonly XDoc Manifest;
        public readonly Stream Data;
        public readonly long DataLength;

        //--- Constructors
        public ImportItem(string dataId, XDoc request, XDoc manifest) : this(dataId, request, manifest, null, 0 ) { }

        public ImportItem(string dataId, XDoc request, XDoc manifest, Stream data, long length) {
            DataId = dataId;
            Request = request;
            Manifest = manifest;
            Data = data;
            DataLength = length;
        }

        //--- Properties ---
        public bool NeedsData { get { return !string.IsNullOrEmpty(DataId) && Data == null; } }

        //--- Methods ---
        public ImportItem WithData(Stream data, long dataLength) {
            return new ImportItem(DataId, Request, Manifest, data, dataLength);
        }

        public void Dispose() {
            Data.Dispose();
        }
    }
}
